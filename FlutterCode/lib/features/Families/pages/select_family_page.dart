// features/auth/presentation/Families/pages/select_family_page.dart - IMPROVED
import 'package:famxpense/core/di/setup_dependency_injection.dart';
import 'package:famxpense/core/theme/app_colors.dart';
import 'package:famxpense/core/router/routes.dart';
import 'package:famxpense/features/Dashboard/cubit/dashboard_cubit.dart';
import 'package:famxpense/features/Invitations/cubit/invitations_cubit.dart';
import 'package:famxpense/features/MyFamily/cubit/my_family_cubit.dart';
import 'package:famxpense/features/Families/Cubits/SelectFamilyState.dart';
import 'package:famxpense/features/Families/Cubits/select_family_cubit.dart';
import 'package:famxpense/core/storage/local_storage.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:go_router/go_router.dart';
import 'package:intl/intl.dart';

class SelectFamilyPage extends StatelessWidget {
  const SelectFamilyPage({super.key});

  @override
  Widget build(BuildContext context) {
    // Use BlocProvider.value since SelectFamilyCubit is a singleton
    return BlocProvider.value(
      value: getIt<SelectFamilyCubit>(),
      child: const _SelectFamilyView(),
    );
  }
}



class _SelectFamilyView extends StatefulWidget {
  const _SelectFamilyView();

  @override
  State<_SelectFamilyView> createState() => _SelectFamilyViewState();
}

class _SelectFamilyViewState extends State<_SelectFamilyView> {
  String? _currentFamilyId;

  @override
  void initState() {
    super.initState();
    _loadCurrentFamilyId();
    // Refresh families data when the page becomes visible
    // This is needed because StatefulShellRoute preserves the widget,
    // so BlocProvider.create doesn't re-run on subsequent navigations.
    WidgetsBinding.instance.addPostFrameCallback((_) {
      if (mounted) {
        context.read<SelectFamilyCubit>().loadFamilies();
      }
    });
  }

  Future<void> _loadCurrentFamilyId() async {
    final id = await getIt<LocalStorage>().getSelectedFamilyId();
    if (mounted) {
      setState(() {
        _currentFamilyId = id;
      });
    }
  }

  void _showDeleteFamilyDialog(dynamic family) {
    showDialog(
      context: context,
      builder: (dialogContext) => AlertDialog(
        backgroundColor: AppColors.surface,
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(16),
        ),
        title: Row(
          children: [
            Icon(
              Icons.delete_forever_rounded,
              color: AppColors.error,
              size: 28,
            ),
            const SizedBox(width: 12),
            const Expanded(
              child: Text(
                'Delete Family',
                style: TextStyle(
                  fontWeight: FontWeight.w700,
                  fontSize: 18,
                ),
              ),
            ),
          ],
        ),
        content: Text(
          'Are you sure you want to permanently delete "${family.name}"?\n\nThis will remove all members and cancel pending invitations. Transactions will be preserved for historical data.',
          style: TextStyle(
            fontSize: 14,
            color: AppColors.textSecondary,
            height: 1.5,
          ),
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(dialogContext),
            child: Text(
              'Cancel',
              style: TextStyle(
                color: AppColors.textSecondary,
                fontWeight: FontWeight.w600,
              ),
            ),
          ),
          ElevatedButton(
            onPressed: () async {
              Navigator.pop(dialogContext);
              await context.read<SelectFamilyCubit>().deleteFamily(family.id);
            },
            style: ElevatedButton.styleFrom(
              backgroundColor: AppColors.error,
              foregroundColor: Colors.white,
              shape: RoundedRectangleBorder(
                borderRadius: BorderRadius.circular(10),
              ),
              padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 10),
            ),
            child: const Text(
              'Delete',
              style: TextStyle(fontWeight: FontWeight.w600),
            ),
          ),
        ],
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
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
                backgroundColor: AppColors.error,
                behavior: SnackBarBehavior.floating,
              ),
            );
          }
        },
        builder: (context, state) {
          if (state is SelectFamilyLoading) {
            return const Center(
              child: CircularProgressIndicator(
                valueColor: AlwaysStoppedAnimation<Color>(AppColors.primary),
              ),
            );
          }

          if (state is SelectFamilyFamiliesLoaded) {
            return SafeArea(
              child: RefreshIndicator(
                onRefresh: () async {
                  context.read<SelectFamilyCubit>().loadFamilies();
                },
                color: AppColors.primary,
                child: CustomScrollView(
                  physics: const AlwaysScrollableScrollPhysics(),
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
                                        color: AppColors.textPrimary,
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
                                        color: AppColors.textSecondary.withValues(alpha: 0.6),
                                        fontWeight: FontWeight.w400,
                                      ),
                                    ),
                                  ],
                                ),
                              ),
                              // Create Family Button (only show when there are families)
                              if (state.families.isNotEmpty)
                                Material(
                                  color: AppColors.primary,
                                  borderRadius: BorderRadius.circular(12),
                                  elevation: 4,
                                  shadowColor: AppColors.primary.withValues(alpha: 0.3),
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
                            final isCurrentFamily = family.id == _currentFamilyId;
                            
                            return Padding(
                              padding: const EdgeInsets.only(bottom: 16),
                              child: _FamilyCard(
                                family: family,
                                isSelected: isCurrentFamily,
                                onTap: () {
                                  if (isCurrentFamily) {
                                    if (context.canPop()) {
                                      context.pop();
                                    } else {
                                      context.go(Routes.dashboard);
                                    }
                                  } else {
                                    context
                                      .read<SelectFamilyCubit>()
                                      .selectFamily(family.id);
                                  }
                                },
                                onDelete: () => _showDeleteFamilyDialog(family),
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
              ),
            );
          }

          // Handle initial state - trigger load
          if (state is SelectFamilyInitial) {
            WidgetsBinding.instance.addPostFrameCallback((_) {
              if (mounted) {
                context.read<SelectFamilyCubit>().loadFamilies();
              }
            });
            return const Center(
              child: CircularProgressIndicator(
                valueColor: AlwaysStoppedAnimation<Color>(AppColors.primary),
              ),
            );
          }

          // Handle error state with retry button
          if (state is SelectFamilyError) {
            return SafeArea(
              child: Center(
                child: Padding(
                  padding: const EdgeInsets.all(32),
                  child: Column(
                    mainAxisAlignment: MainAxisAlignment.center,
                    children: [
                      Icon(
                        Icons.error_outline_rounded,
                        size: 64,
                        color: AppColors.error.withOpacity(0.6),
                      ),
                      const SizedBox(height: 16),
                      const Text(
                        'Failed to load families',
                        style: TextStyle(
                          fontSize: 18,
                          fontWeight: FontWeight.w600,
                          color: AppColors.textPrimary,
                        ),
                      ),
                      const SizedBox(height: 8),
                      Text(
                        state.message,
                        textAlign: TextAlign.center,
                        style: TextStyle(
                          fontSize: 14,
                          color: AppColors.textSecondary,
                        ),
                      ),
                      const SizedBox(height: 24),
                      ElevatedButton.icon(
                        onPressed: () {
                          context.read<SelectFamilyCubit>().loadFamilies();
                        },
                        style: ElevatedButton.styleFrom(
                          backgroundColor: AppColors.primary,
                          foregroundColor: Colors.white,
                          padding: const EdgeInsets.symmetric(
                            horizontal: 24,
                            vertical: 12,
                          ),
                          shape: RoundedRectangleBorder(
                            borderRadius: BorderRadius.circular(12),
                          ),
                        ),
                        icon: const Icon(Icons.refresh_rounded),
                        label: const Text('Retry'),
                      ),
                    ],
                  ),
                ),
              ),
            );
          }

          // Handle success state - should navigate away, but if still here reload
          if (state is SelectFamilySuccess) {
            WidgetsBinding.instance.addPostFrameCallback((_) {
              if (mounted) {
                context.read<SelectFamilyCubit>().loadFamilies();
              }
            });
            return const Center(
              child: CircularProgressIndicator(
                valueColor: AlwaysStoppedAnimation<Color>(AppColors.primary),
              ),
            );
          }

          return const Center(
            child: Text('Something went wrong'),
          );
        },
      ),
      // Only show bottom bar for navigation to Invitations
      // Using a simplified navigation bar manually since we are in the shell or not?
      // Wait, if we are in the shell, the shell handles the bottom bar.
      // But if AppRouter doesn't consistently show it for SelectFamily, we might need a button.
      // However, if we are in Branch 5, the SHELL should show it.
      // IF SelectFamilyPage is used OUTSIDE the shell (routes: ... GoRoute(path: Routes.manageFamilies...)), then no shell.
      //
      // BUT, checking AppRouter:
      // Branch 5 -> GoRoute(path: Routes.selectFamily ... builder: SelectFamilyPage)
      // Root -> GoRoute(path: Routes.manageFamilies ... builder: SelectFamilyPage)
      //
      // If user is redirected to Routes.selectFamily, they are in the SHELL.
      // So the shell SHOULD show the bottom bar.
      // Why didn't I see it in my mental model? 
      // Because `ScaffoldWithNestedNavigation` logic for `currentIndex == 5` sets `isFamilySelected = false`.
      // `AppBottomNavBar` with `isFamilySelected = false` shows: [Families, Invitations].
      //
      // If the user navigates to `Routes.selectFamily`, the shell should render ScaffolWithNestedNavigation -> Scaffold -> body: SelectFamilyPage, bottomBar: AppBottomNavBar.
      // SelectFamilyPage ALSO returns a Scaffold.
      // Nested Scaffolds are okay.
      //
      // So... the bottom bar SHOULD be there.
      //
      // One possibility: User is at `Routes.manageFamilies` (ROOT route), NOT `Routes.selectFamily` (SHELL route).
      //
      // AppRouter redirect:
      // if (requiresFamilySelection && no family) -> return Routes.selectFamily; (Shell route!)
      //
      // So they ARE in the shell.
      //
      // Maybe I should assume the bottom bar IS showing, but the user didn't notice "Invitations" tab?
      // Or maybe the icons are confusing?
      //
      // But adding a direct link in `_EmptyFamiliesView` is safe.
      // "Check Invitations" button.
      
    );
  }
}

class _FamilyCard extends StatelessWidget {
  final dynamic family;
  final VoidCallback onTap;
  final VoidCallback? onDelete;
  final bool isSelected;

  const _FamilyCard({
    required this.family,
    required this.onTap,
    this.onDelete,
    this.isSelected = false,
  });

  @override
  Widget build(BuildContext context) {
    final currency = NumberFormat.simpleCurrency();
    final memberCount = family.members?.length ?? 0;

    return Container(
      decoration: BoxDecoration(
        color: AppColors.white,
        borderRadius: BorderRadius.circular(12),
        border: Border.all(
          color: isSelected ? AppColors.primary : AppColors.border,
          width: isSelected ? 2.5 : 1.5,
        ),
        boxShadow: [
          BoxShadow(
            color: isSelected 
                ? AppColors.primary.withOpacity(0.15) 
                : Colors.black.withValues(alpha: 0.05),
            blurRadius: isSelected ? 16 : 10,
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
                              color: AppColors.textPrimary,
                            ),
                          ),
                          const SizedBox(height: 6),
                          Text(
                            currency.format(family.currentBudget),
                            style: const TextStyle(
                              fontSize: 16,
                              fontWeight: FontWeight.w600,
                              color: AppColors.success,
                            ),
                          ),
                        ],
                      ),
                    ),
                    // Delete button
                    if (onDelete != null)
                      IconButton(
                        onPressed: onDelete,
                        style: IconButton.styleFrom(
                          backgroundColor: AppColors.error.withOpacity(0.08),
                          padding: const EdgeInsets.all(8),
                        ),
                        icon: Icon(
                          Icons.delete_outline_rounded,
                          color: AppColors.error,
                          size: 20,
                        ),
                        tooltip: 'Delete family',
                      ),
                    const SizedBox(width: 8),
                    const Icon(
                      Icons.arrow_forward_ios,
                      size: 18,
                      color: AppColors.primary,
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
                        color: AppColors.textSecondary.withValues(alpha: 0.7),
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
                      color: AppColors.textSecondary.withValues(alpha: 0.6),
                    ),
                    const SizedBox(width: 6),
                    Text(
                      '$memberCount ${memberCount == 1 ? 'Member' : 'Members'}',
                      style: TextStyle(
                        fontSize: 13,
                        fontWeight: FontWeight.w600,
                        color: AppColors.textSecondary.withValues(alpha: 0.7),
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
                color: AppColors.primary.withValues(alpha: 0.1),
                shape: BoxShape.circle,
              ),
              child: const Icon(
                Icons.family_restroom,
                size: 40,
                color: AppColors.primary,
              ),
            ),
            const SizedBox(height: 24),
            const Text(
              'No Families Yet',
              style: TextStyle(
                fontSize: 22,
                fontWeight: FontWeight.w700,
                color: AppColors.textPrimary,
              ),
            ),
            const SizedBox(height: 12),
            Text(
              'Create your first family to start managing expenses together.',
              textAlign: TextAlign.center,
              style: TextStyle(
                fontSize: 14,
                color: AppColors.textSecondary.withValues(alpha: 0.7),
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
                  backgroundColor: AppColors.primary,
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

