import 'package:famxpense/core/di/setup_dependency_injection.dart';
import 'package:famxpense/core/theme/app_colors.dart';
import 'package:famxpense/core/router/routes.dart';
import 'package:famxpense/features/Dashboard/cubit/dashboard_cubit.dart';
import 'package:famxpense/features/Invitations/cubit/invitations_cubit.dart';
import 'package:famxpense/features/MyFamily/cubit/my_family_cubit.dart';
import 'package:famxpense/features/Families/Cubits/SelectFamilyState.dart';
import 'package:famxpense/features/Families/Cubits/select_family_cubit.dart';
import 'package:famxpense/core/storage/local_storage.dart';
import 'package:famxpense/l10n/app_localizations.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:go_router/go_router.dart';
import 'package:intl/intl.dart';

class SelectFamilyPage extends StatelessWidget {
  const SelectFamilyPage({super.key});

  @override
  Widget build(BuildContext context) {
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
    final l10n = AppLocalizations.of(context)!;
    
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
            Expanded(
              child: Text(
                l10n.deleteFamily,
                style: const TextStyle(
                  fontWeight: FontWeight.w700,
                  fontSize: 18,
                ),
              ),
            ),
          ],
        ),
        content: Text(
          l10n.deleteFamilyConfirmMessage(family.name),
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
              l10n.cancel,
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
            child: Text(
              l10n.delete,
              style: const TextStyle(fontWeight: FontWeight.w600),
            ),
          ),
        ],
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context)!;
    
    return Scaffold(
      backgroundColor: AppColors.background,
      body: BlocConsumer<SelectFamilyCubit, SelectFamilyState>(
        listener: (context, state) async {
          if (state is SelectFamilySuccess) {
      
            await Future.delayed(const Duration(milliseconds: 100));
            
            getIt<InvitationsCubit>().loadAll();
            getIt<MyFamilyCubit>().loadFamilyDetails();
            
            await getIt<DashboardCubit>().loadDashboard();
            
            if (context.mounted) {
              context.go(Routes.dashboard);
            }
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
                                    Text(
                                      l10n.selectAFamily,
                                      style: const TextStyle(
                                        fontSize: 28,
                                        fontWeight: FontWeight.w700,
                                        color: AppColors.textPrimary,
                                        letterSpacing: -0.5,
                                      ),
                                    ),
                                    const SizedBox(height: 8),
                                    Text(
                                      state.families.isEmpty
                                          ? l10n.createYourFirstFamily
                                          : l10n.chooseFamily,
                                      style: TextStyle(
                                        fontSize: 15,
                                        color: AppColors.textSecondary.withValues(alpha: 0.6),
                                        fontWeight: FontWeight.w400,
                                      ),
                                    ),
                                  ],
                                ),
                              ),
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
                    SliverFillRemaining(
                      child: _EmptyFamiliesView(l10n: l10n),
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
                                l10n: l10n,
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
                      Text(
                        l10n.failedToLoadFamilies,
                        style: const TextStyle(
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
                        label: Text(l10n.retry),
                      ),
                    ],
                  ),
                ),
              ),
            );
          }

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

          return Center(
            child: Text(l10n.somethingWentWrong),
          );
        },
      ),
    );
  }
}

class _FamilyCard extends StatelessWidget {
  final dynamic family;
  final VoidCallback onTap;
  final VoidCallback? onDelete;
  final bool isSelected;
  final AppLocalizations l10n;

  const _FamilyCard({
    required this.family,
    required this.onTap,
    this.onDelete,
    this.isSelected = false,
    required this.l10n,
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
                        tooltip: l10n.deleteFamily,
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
                      l10n.memberCount(memberCount),
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
  final AppLocalizations l10n;
  
  const _EmptyFamiliesView({required this.l10n});

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
            Text(
              l10n.noFamiliesYet,
              style: const TextStyle(
                fontSize: 22,
                fontWeight: FontWeight.w700,
                color: AppColors.textPrimary,
              ),
            ),
            const SizedBox(height: 12),
            Text(
              l10n.createFirstFamilyHint,
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
                label: Text(
                  l10n.createYourFirstFamilyButton,
                  style: const TextStyle(
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

