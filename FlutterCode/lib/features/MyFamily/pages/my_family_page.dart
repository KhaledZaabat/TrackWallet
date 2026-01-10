import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:go_router/go_router.dart';

import 'package:famxpense/core/di/setup_dependency_injection.dart';
import 'package:famxpense/core/theme/app_colors.dart';
import 'package:famxpense/core/storage/local_storage.dart';
import 'package:famxpense/features/MyFamily/cubit/my_family_cubit.dart';
import 'package:famxpense/features/MyFamily/cubit/my_family_state.dart';
import 'package:famxpense/features/Transactions/Cubits/transaction_cubit.dart';
import 'package:famxpense/features/Transactions/Cubits/transaction_state.dart';
import 'package:famxpense/features/MyFamily/widgets/family_header.dart';
import 'package:famxpense/features/MyFamily/widgets/members_list.dart';
import 'package:famxpense/features/MyFamily/widgets/edit_family_dialog.dart';
import 'package:famxpense/features/Families/Cubits/select_family_cubit.dart';
import 'package:famxpense/core/router/routes.dart';
import 'package:famxpense/l10n/app_localizations.dart';

/// MyFamily page displays all members of the currently selected family
///
/// Features:
/// - Family header with name, budget, bio, and edit button
/// - List of all family members with profile information
/// - Kick member functionality (parents only)
/// - Edit family info (parents only)
class MyFamilyPage extends StatelessWidget {
  const MyFamilyPage({super.key});

  @override
  Widget build(BuildContext context) {
    return BlocProvider.value(
      value: getIt<MyFamilyCubit>(),
      child: const _MyFamilyView(),
    );
  }
}

class _MyFamilyView extends StatefulWidget {
  const _MyFamilyView();

  @override
  State<_MyFamilyView> createState() => _MyFamilyViewState();
}

class _MyFamilyViewState extends State<_MyFamilyView> {
  String? _currentUserId;

  @override
  void initState() {
    super.initState();
    _loadCurrentUser();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      if (mounted) {
        context.read<MyFamilyCubit>().loadFamilyDetails();
      }
    });
  }

  Future<void> _loadCurrentUser() async {
    final userId = await getIt<LocalStorage>().getUserId();
    if (mounted) {
      setState(() {
        _currentUserId = userId;
      });
    }
  }

  void _showEditFamilyDialog(MyFamilyLoaded state) {
    showDialog(
      context: context,
      builder: (_) => EditFamilyDialog(
        currentName: state.familyDetails.name,
        currentBio: state.familyDetails.familyBio,
        onSave: (name, bio) {
          context.read<MyFamilyCubit>().updateFamilyInfo(name: name, bio: bio);
        },
      ),
    );
  }

  void _showLeaveFamilyDialog() {
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
              Icons.exit_to_app_rounded,
              color: AppColors.error,
              size: 28,
            ),
            const SizedBox(width: 12),
            Expanded(
              child: Text(
                l10n.leaveFamily,
                style: GoogleFonts.inter(
                  fontWeight: FontWeight.w700,
                  fontSize: 18,
                ),
              ),
            ),
          ],
        ),
        content: Text(
          l10n.leaveFamilyConfirmMessage,
          style: GoogleFonts.inter(
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
              style: GoogleFonts.inter(
                color: AppColors.textSecondary,
                fontWeight: FontWeight.w600,
              ),
            ),
          ),
          ElevatedButton(
            onPressed: () async {
              Navigator.pop(dialogContext);
              final success = await context.read<MyFamilyCubit>().leaveFamily();
              if (success && mounted) {
                // Refresh families list before navigating
                getIt<SelectFamilyCubit>().loadFamilies();
                context.go(Routes.selectFamily);
              }
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
              l10n.leave,
              style: GoogleFonts.inter(
                fontWeight: FontWeight.w600,
              ),
            ),
          ),
        ],
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context)!;
    
    return BlocListener<TransactionCubit, TransactionState>(
      bloc: getIt<TransactionCubit>(),
      listener: (context, state) {
        if (state is TransactionOperationSuccess) {
          context.read<MyFamilyCubit>().loadFamilyDetails();
        }
      },
      child: Scaffold(
        backgroundColor: AppColors.background,
        appBar: AppBar(
          title: Text(
            l10n.myFamily,
            style: GoogleFonts.inter(
              fontWeight: FontWeight.w700,
            ),
          ),
          centerTitle: false,
          backgroundColor: AppColors.surface,
          elevation: 0,
          surfaceTintColor: Colors.transparent,
          foregroundColor: AppColors.textPrimary,
        ),
        body: BlocConsumer<MyFamilyCubit, MyFamilyState>(
          listener: (context, state) {
            if (state is MyFamilyError) {
              ScaffoldMessenger.of(context).showSnackBar(
                SnackBar(
                  content: Text(state.message),
                  backgroundColor: AppColors.error,
                  behavior: SnackBarBehavior.floating,
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(10),
                  ),
                ),
              );
            }
            if (state is MyFamilyOperationSuccess) {
              ScaffoldMessenger.of(context).showSnackBar(
                SnackBar(
                  content: Text(state.message),
                  backgroundColor: AppColors.success,
                  behavior: SnackBarBehavior.floating,
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(10),
                  ),
                ),
              );
            }
          },
          builder: (context, state) {
            if (state is MyFamilyLoading) {
              return const Center(
                child: CircularProgressIndicator(
                  valueColor: AlwaysStoppedAnimation<Color>(AppColors.primary),
                ),
              );
            }

            if (state is MyFamilyLoaded || state is MyFamilyOperationSuccess) {
              final familyDetails = state is MyFamilyLoaded
                  ? state.familyDetails
                  : (state as MyFamilyOperationSuccess).familyDetails;
              final isParent = state is MyFamilyLoaded
                  ? state.isCurrentUserParent
                  : (state as MyFamilyOperationSuccess).isCurrentUserParent;
              final operationInProgress = state is MyFamilyLoaded
                  ? state.operationInProgress
                  : null;

              return RefreshIndicator(
                onRefresh: () async {
                  context.read<MyFamilyCubit>().loadFamilyDetails();
                },
                color: AppColors.primary,
                child: SingleChildScrollView(
                  physics: const AlwaysScrollableScrollPhysics(),
                  child: Column(
                    children: [
                      FamilyHeader(
                        familyDetails: familyDetails,
                        isCurrentUserParent: isParent,
                        onEditPressed: isParent
                            ? () => _showEditFamilyDialog(
                                  MyFamilyLoaded(
                                    familyDetails: familyDetails,
                                    isCurrentUserParent: isParent,
                                  ),
                                )
                            : null,
                      ),
                      MembersListWidget(
                        members: familyDetails.members,
                        isCurrentUserParent: isParent,
                        currentUserId: _currentUserId,
                        operationInProgress: operationInProgress,
                        onKickMember: (userId) {
                          context.read<MyFamilyCubit>().kickMember(userId);
                        },
                      ),
                      const SizedBox(height: 24),
                      // Leave Family Button
                      Padding(
                        padding: const EdgeInsets.symmetric(horizontal: 20),
                        child: OutlinedButton.icon(
                          onPressed: _showLeaveFamilyDialog,
                          style: OutlinedButton.styleFrom(
                            foregroundColor: AppColors.error,
                            side: BorderSide(color: AppColors.error.withOpacity(0.5)),
                            padding: const EdgeInsets.symmetric(vertical: 17),
                            shape: RoundedRectangleBorder(
                              borderRadius: BorderRadius.circular(12),
                            ),
                          ),
                          icon: const Icon(Icons.exit_to_app_rounded),
                          label: Text(
                            l10n.leaveFamily,
                            style: GoogleFonts.inter(
                              fontWeight: FontWeight.w600,
                              fontSize: 15,
                            ),
                          ),
                        ),
                      ),
                      const SizedBox(height: 100),
                    ],
                  ),
                ),
              );
            }

            if (state is MyFamilyError) {
              return Center(
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
                        l10n.failedToLoadFamily,
                        style: GoogleFonts.inter(
                          fontSize: 18,
                          fontWeight: FontWeight.w600,
                          color: AppColors.textPrimary,
                        ),
                      ),
                      const SizedBox(height: 8),
                      Text(
                        state.message,
                        textAlign: TextAlign.center,
                        style: GoogleFonts.inter(
                          fontSize: 14,
                          color: AppColors.textSecondary,
                        ),
                      ),
                      const SizedBox(height: 24),
                      ElevatedButton.icon(
                        onPressed: () {
                          context.read<MyFamilyCubit>().loadFamilyDetails();
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
              );
            }

            return const SizedBox.shrink();
          },
        ),
      ),
    );
  }
}
