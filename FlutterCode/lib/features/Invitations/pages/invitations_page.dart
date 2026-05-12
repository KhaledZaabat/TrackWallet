import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:go_router/go_router.dart';
import 'package:famxpense/core/router/routes.dart';
import 'package:famxpense/core/storage/local_storage.dart';
import 'package:famxpense/core/di/setup_dependency_injection.dart';
import 'package:famxpense/core/theme/app_colors.dart';
import 'package:famxpense/features/Invitations/cubit/invitations_cubit.dart';
import 'package:famxpense/features/Invitations/cubit/invitations_state.dart';
import 'package:famxpense/features/Invitations/widgets/received_invitations_tab.dart';
import 'package:famxpense/features/Invitations/widgets/sent_invitations_tab.dart';
import 'package:famxpense/features/Invitations/widgets/send_invitation_dialog.dart';
import 'package:famxpense/l10n/app_localizations.dart';

class InvitationsPage extends StatefulWidget {
  final bool forceGuestMode;
  const InvitationsPage({Key? key, this.forceGuestMode = false}) : super(key: key);

  @override
  State<InvitationsPage> createState() => _InvitationsPageState();
}

class _InvitationsPageState extends State<InvitationsPage> {
  bool _isFamilySelected = false;


  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      if (mounted) {
        getIt<LocalStorage>().getSelectedFamilyId().then((familyId) {
          setState(() {
            _isFamilySelected = !widget.forceGuestMode && familyId != null && familyId.isNotEmpty;
          });
        });
        context.read<InvitationsCubit>().loadAll();
      }
    });
  }



  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context)!;
    
    return BlocConsumer<InvitationsCubit, InvitationsState>(
      listener: (context, state) {
        if (state is InvitationsError) {
          final cubit = context.read<InvitationsCubit>();
          
          ScaffoldMessenger.of(context).showSnackBar(
            SnackBar(
              content: Text(state.message),
              duration: const Duration(seconds: 5),
              action: SnackBarAction(
                label: l10n.retry,
                onPressed: () => cubit.loadAll(),
              ),
            ),
          );
        }
      },
      builder: (context, state) {
        if (state is InvitationsLoading) {
          return Scaffold(
            backgroundColor: AppColors.background,
            appBar: AppBar(
              title: Text(l10n.familyInvitations),
              backgroundColor: AppColors.surface,
              foregroundColor: AppColors.textPrimary,
              elevation: 0,
              automaticallyImplyLeading: false,
            ),
            body: const Center(
              child: CircularProgressIndicator(),
            ),
          );
        }

        if (state is InvitationsError) {
          return Scaffold(
             appBar: AppBar(
              title: Text(l10n.familyInvitations),
              backgroundColor: AppColors.surface,
              foregroundColor: AppColors.textPrimary,
              elevation: 0,
              automaticallyImplyLeading: false,
            ),
            backgroundColor: AppColors.background,
            body: Center(
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  Icon(
                    Icons.error_outline,
                    size: 64,
                    color: AppColors.error,
                  ),
                  const SizedBox(height: 16),
                  Text(
                    state.message,
                    textAlign: TextAlign.center,
                    style: Theme.of(context).textTheme.bodyLarge,
                  ),
                  const SizedBox(height: 24),
                  ElevatedButton(
                    onPressed: () =>
                        context.read<InvitationsCubit>().loadAll(),
                    style: ElevatedButton.styleFrom(
                      backgroundColor: AppColors.primary,
                      foregroundColor: Colors.white,
                    ),
                    child: Text(l10n.retry),
                  ),
                ],
              ),
            ),
          );
        }

        if (state is InvitationsLoaded) {
          final currentUserEmail = '';
          final cubit = context.read<InvitationsCubit>();

          if (!_isFamilySelected) {
            return Scaffold(
              backgroundColor: AppColors.background,
              appBar: AppBar(
                title: Text(l10n.familyInvitations),
                backgroundColor: AppColors.surface,
                foregroundColor: AppColors.textPrimary,
                elevation: 0,
              ),
              body: ReceivedInvitationsTab(
                invitations: state.receivedInvitations,
                loadingInvitationId: state.loadingInvitationId,
                onAccept: (id) =>
                    context.read<InvitationsCubit>().acceptInvitation(id),
                onDecline: (id) =>
                    context.read<InvitationsCubit>().declineInvitation(id),
                onRefresh: () async {
                  await context.read<InvitationsCubit>().loadAll();
                },
              ),
            );
          }

          return DefaultTabController(
            length: 2,
            initialIndex: state.selectedTab,
            child: Scaffold(
              backgroundColor: AppColors.background,
              appBar: AppBar(
                title: Text(l10n.familyInvitations),
                backgroundColor: AppColors.surface,
                foregroundColor: AppColors.textPrimary,
                elevation: 0,
                automaticallyImplyLeading: false,
                bottom: TabBar(
                  onTap: (index) =>
                      context.read<InvitationsCubit>().switchTab(index),
                  labelColor: AppColors.primary,
                  unselectedLabelColor: AppColors.textSecondary,
                  indicatorColor: AppColors.primary,
                  tabs: [
                    Tab(text: l10n.received),
                    Tab(text: l10n.sent),
                  ],
                ),
              ),
              body: TabBarView(
                children: [
                  ReceivedInvitationsTab(
                    invitations: state.receivedInvitations,
                    loadingInvitationId: state.loadingInvitationId,
                    onAccept: (id) =>
                        context.read<InvitationsCubit>().acceptInvitation(id),
                    onDecline: (id) =>
                        context.read<InvitationsCubit>().declineInvitation(id),
                    onRefresh: () async {
                      await context.read<InvitationsCubit>().loadAll();
                    },
                  ),
                  SentInvitationsTab(
                    invitations: state.sentInvitations,
                    loadingInvitationId: state.loadingInvitationId,
                    onCancel: (id) =>
                        context.read<InvitationsCubit>().cancelInvitation(id),
                  ),
                ],
              ),
              floatingActionButton: FloatingActionButton(
                backgroundColor: AppColors.primary,
                foregroundColor: Colors.white,
                onPressed: () {
                  showDialog(
                    context: context,
                    builder: (dialogContext) => SendInvitationDialog(
                      currentUserEmail: currentUserEmail,
                      cubit: cubit,
                    ),
                  );
                },
                tooltip: l10n.sendInvitation,
                child: const Icon(Icons.mail_outline),
              ),
            ),
          );
        }

        return Scaffold(
            backgroundColor: AppColors.background,
            appBar: AppBar(
              title: Text(l10n.familyInvitations),
              backgroundColor: AppColors.surface,
              foregroundColor: AppColors.textPrimary,
              elevation: 0,
              automaticallyImplyLeading: false,
            ),
          body: const Center(
            child: CircularProgressIndicator(),
          ),
        );
      },
    );
  }
}
