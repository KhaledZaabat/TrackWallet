// features/auth/presentation/Families/pages/select_family_page.dart - IMPROVED
import 'package:famxpense/core/di/setup_dependency_injection.dart';
import 'package:famxpense/core/router/routes.dart';
import 'package:famxpense/features/auth/presentation/Dashboard/cubit/dashboard_cubit.dart';
import 'package:famxpense/features/Invitations/cubit/invitations_cubit.dart';
import 'package:famxpense/features/MyFamily/cubit/my_family_cubit.dart';
import 'package:famxpense/features/auth/presentation/Families/Cubits/SelectFamilyState.dart';
import 'package:famxpense/features/auth/presentation/Families/Cubits/select_family_cubit.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:go_router/go_router.dart';
import 'package:intl/intl.dart';

class SelectFamilyPage extends StatelessWidget {
  const SelectFamilyPage({super.key});

  @override
  Widget build(BuildContext context) {
    return BlocProvider(
      create: (_) => getIt<SelectFamilyCubit>()..loadFamilies(),
      child: const _SelectFamilyView(),
    );
  }
}

class _SelectFamilyView extends StatelessWidget {
  const _SelectFamilyView();

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFFF5F8FA),
      body: BlocConsumer<SelectFamilyCubit, SelectFamilyState>(
        listener: (context, state) {
          if (state is SelectFamilySuccess) {
            // Reset cubits to clear cached data from previous family
            getIt<InvitationsCubit>().loadAll();
            getIt<DashboardCubit>().loadDashboard();
            getIt<MyFamilyCubit>().loadFamilyDetails();
            
            // Navigate to dashboard
            context.go('/dashboard');
          }
          if (state is SelectFamilyError) {
            ScaffoldMessenger.of(context).showSnackBar(
              SnackBar(
                content: Text(state.message),
                backgroundColor: Colors.red,
                behavior: SnackBarBehavior.floating,
              ),
            );
          }
        },
        builder: (context, state) {
          if (state is SelectFamilyLoading) {
            return const Center(
              child: CircularProgressIndicator(
                valueColor: AlwaysStoppedAnimation<Color>(Color(0xFF5B7CB5)),
              ),
            );
          }

          if (state is SelectFamilyFamiliesLoaded) {
            return SafeArea(
              child: CustomScrollView(
                slivers: [
                  SliverToBoxAdapter(
                    child: Padding(
                      padding: const EdgeInsets.fromLTRB(24, 40, 24, 20),
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Row(
                            mainAxisAlignment: MainAxisAlignment.spaceBetween,
                            children: [
                              Expanded(
                                child: Column(
                                  crossAxisAlignment: CrossAxisAlignment.start,
                                  children: [
                                    const Text(
                                      'Select a Family',
                                      style: TextStyle(
                                        fontSize: 28,
                                        fontWeight: FontWeight.w700,
                                        color: Color(0xFF5B6B8C),
                                        letterSpacing: -0.5,
                                      ),
                                    ),
                                    const SizedBox(height: 8),
                                    Text(
                                      state.families.isEmpty
                                          ? 'Create your first family'
                                          : 'Choose a family to continue',
                                      style: TextStyle(
                                        fontSize: 15,
                                        color: const Color(0xFF5B6B8C)
                                            .withValues(alpha: 0.6),
                                        fontWeight: FontWeight.w400,
                                      ),
                                    ),
                                  ],
                                ),
                              ),
                              // Create Family Button (Floating Action Button style)
                              Container(
                                decoration: BoxDecoration(
                                  color: const Color(0xFF5B7CB5),
                                  borderRadius: BorderRadius.circular(12),
                                  boxShadow: [
                                    BoxShadow(
                                      color: const Color(0xFF5B7CB5)
                                          .withValues(alpha: 0.3),
                                      blurRadius: 12,
                                      offset: const Offset(0, 4),
                                    ),
                                  ],
                                ),
                                child: Material(
                                  color: Colors.transparent,
                                  child: InkWell(
                                    onTap: () async {
                                      final result = await context.push(
                                        Routes.createFamily,
                                      );

                                      // Reload families if a family was created
                                      if (result == true && context.mounted) {
                                        context
                                            .read<SelectFamilyCubit>()
                                            .loadFamilies();
                                      }
                                    },
                                    borderRadius: BorderRadius.circular(12),
                                    child: const Padding(
                                      padding: EdgeInsets.all(12),
                                      child: Icon(
                                        Icons.add,
                                        color: Colors.white,
                                        size: 24,
                                      ),
                                    ),
                                  ),
                                ),
                              ),
                            ],
                          ),
                        ],
                      ),
                    ),
                  ),
                  if (state.families.isEmpty)
                    const SliverFillRemaining(
                      child: _EmptyFamiliesView(),
                    )
                  else
                    SliverPadding(
                      padding: const EdgeInsets.symmetric(horizontal: 24),
                      sliver: SliverList(
                        delegate: SliverChildBuilderDelegate(
                          (context, index) {
                            final family = state.families[index];
                            return Padding(
                              padding: const EdgeInsets.only(bottom: 16),
                              child: _FamilyCard(
                                family: family,
                                onTap: () {
                                  context
                                      .read<SelectFamilyCubit>()
                                      .selectFamily(family.id);
                                },
                              ),
                            );
                          },
                          childCount: state.families.length,
                        ),
                      ),
                    ),
                  const SliverToBoxAdapter(
                    child: SizedBox(height: 24),
                  ),
                ],
              ),
            );
          }

          return const Center(
            child: Text('Something went wrong'),
          );
        },
      ),
      bottomNavigationBar: BottomNavigationBar(
        currentIndex: 0,
        type: BottomNavigationBarType.fixed,
        backgroundColor: Colors.white,
        selectedItemColor: const Color(0xFF5B7CB5),
        unselectedItemColor: const Color(0xFF5B6B8C).withOpacity(0.6),
        onTap: (index) {
          switch (index) {
            case 0:
              // Already on select family page
              break;
            case 1:
              context.go(Routes.invitationsToJoin);
              break;
          }
        },
        items: const [
          BottomNavigationBarItem(
            icon: Icon(Icons.people),
            label: 'Families',
          ),
          BottomNavigationBarItem(
            icon: Icon(Icons.mail),
            label: 'Invitations',
          ),
        ],
      ),
    );
  }
}

class _FamilyCard extends StatelessWidget {
  final dynamic family;
  final VoidCallback onTap;

  const _FamilyCard({
    required this.family,
    required this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    final currency = NumberFormat.simpleCurrency();
    final memberCount = family.members?.length ?? 0;

    return Container(
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(12),
        border: Border.all(
          color: const Color(0xFFE0E5EB),
          width: 1.5,
        ),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withValues(alpha: 0.05),
            blurRadius: 10,
            offset: const Offset(0, 4),
          ),
        ],
      ),
      child: Material(
        color: Colors.transparent,
        child: InkWell(
          onTap: onTap,
          borderRadius: BorderRadius.circular(12),
          child: Padding(
            padding: const EdgeInsets.all(20),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Row(
                  children: [
                    Expanded(
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text(
                            family.name,
                            style: const TextStyle(
                              fontSize: 20,
                              fontWeight: FontWeight.w700,
                              color: Color(0xFF5B6B8C),
                            ),
                          ),
                          const SizedBox(height: 6),
                          Text(
                            currency.format(family.currentBudget),
                            style: const TextStyle(
                              fontSize: 16,
                              fontWeight: FontWeight.w600,
                              color: Color(0xFF27AE60),
                            ),
                          ),
                        ],
                      ),
                    ),
                    const Icon(
                      Icons.arrow_forward_ios,
                      size: 18,
                      color: Color(0xFF5B7CB5),
                    ),
                  ],
                ),
                if (family.familyBio != null && family.familyBio!.isNotEmpty)
                  Padding(
                    padding: const EdgeInsets.only(top: 12),
                    child: Text(
                      family.familyBio!,
                      style: TextStyle(
                        fontSize: 13,
                        color: const Color(0xFF5B6B8C).withValues(alpha: 0.7),
                      ),
                      maxLines: 2,
                      overflow: TextOverflow.ellipsis,
                    ),
                  ),
                const SizedBox(height: 16),
                Row(
                  children: [
                    Icon(
                      Icons.people_outline,
                      size: 16,
                      color: const Color(0xFF5B6B8C).withValues(alpha: 0.6),
                    ),
                    const SizedBox(width: 6),
                    Text(
                      '$memberCount ${memberCount == 1 ? 'Member' : 'Members'}',
                      style: TextStyle(
                        fontSize: 13,
                        fontWeight: FontWeight.w600,
                        color: const Color(0xFF5B6B8C).withValues(alpha: 0.7),
                      ),
                    ),
                  ],
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}

class _EmptyFamiliesView extends StatelessWidget {
  const _EmptyFamiliesView();

  @override
  Widget build(BuildContext context) {
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(32),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Container(
              width: 80,
              height: 80,
              decoration: BoxDecoration(
                color: const Color(0xFF5B7CB5).withValues(alpha: 0.1),
                shape: BoxShape.circle,
              ),
              child: const Icon(
                Icons.family_restroom,
                size: 40,
                color: Color(0xFF5B7CB5),
              ),
            ),
            const SizedBox(height: 24),
            const Text(
              'No Families Yet',
              style: TextStyle(
                fontSize: 22,
                fontWeight: FontWeight.w700,
                color: Color(0xFF5B6B8C),
              ),
            ),
            const SizedBox(height: 12),
            Text(
              'Create your first family to start managing expenses together.',
              textAlign: TextAlign.center,
              style: TextStyle(
                fontSize: 14,
                color: const Color(0xFF5B6B8C).withValues(alpha: 0.7),
              ),
            ),
            const SizedBox(height: 32),
            SizedBox(
              width: double.infinity,
              height: 50,
              child: ElevatedButton.icon(
                onPressed: () async {
                  final result = await context.push(Routes.createFamily);

                  // Reload families if a family was created
                  if (result == true && context.mounted) {
                    context.read<SelectFamilyCubit>().loadFamilies();
                  }
                },
                style: ElevatedButton.styleFrom(
                  backgroundColor: const Color(0xFF5B7CB5),
                  foregroundColor: Colors.white,
                  elevation: 0,
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(8),
                  ),
                ),
                icon: const Icon(Icons.add),
                label: const Text(
                  'Create Your First Family',
                  style: TextStyle(
                    fontSize: 16,
                    fontWeight: FontWeight.w600,
                  ),
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }
}

