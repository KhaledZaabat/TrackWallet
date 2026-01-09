import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:go_router/go_router.dart';
import 'package:famxpense/core/router/routes.dart';
import 'package:famxpense/core/storage/local_storage.dart';
import 'package:famxpense/core/di/setup_dependency_injection.dart';
import 'package:famxpense/features/Invitations/cubit/invitations_cubit.dart';
import 'package:famxpense/features/Invitations/cubit/invitations_state.dart';

import 'package:famxpense/features/Invitations/widgets/received_invitations_tab.dart';
import 'package:famxpense/features/Invitations/widgets/sent_invitations_tab.dart';
import 'package:famxpense/features/Invitations/widgets/send_invitation_dialog.dart';

/// Main page for managing family invitations with tabbed interface
///
/// Features:
/// - Tabbed interface: Received (Pending only) and Sent (all statuses)
/// - Accept/Decline buttons for received invitations
/// - Cancel button for pending sent invitations
/// - Floating Action Button to send new invitations
/// - Real-time loading states for individual invitation actions
/// - Error handling with retry mechanism
/// - Tab persistence (switches tabs without reloading data)
///
/// State Management:
/// - InvitationsInitial: Initial state before loading
/// - InvitationsLoading: Loading both lists
/// - InvitationsLoaded: Both lists loaded with tab info
/// - InvitationsError: Error state with message
///
/// Usage:
/// - Place in routes as '/invitations' route
/// - Accessible after family selection
/// - Automatically loads data on page open (via initState)
class InvitationsPage extends StatefulWidget {
  const InvitationsPage({Key? key}) : super(key: key);

  @override
  State<InvitationsPage> createState() => _InvitationsPageState();
}

class _InvitationsPageState extends State<InvitationsPage> {
  bool _isFamilySelected = true; // Default to true, will check on init

  @override
  void initState() {
    super.initState();
    // Load invitations data after first frame builds
    WidgetsBinding.instance.addPostFrameCallback((_) {
      if (mounted) {
        // Check if family is selected
        getIt<LocalStorage>().getSelectedFamilyId().then((familyId) {
          setState(() {
            _isFamilySelected = familyId != null && familyId.isNotEmpty;
          });
        });
        context.read<InvitationsCubit>().loadAll();
      }
    });
  }

  /// Determine which navbar item should be selected based on current route
  int _getCurrentNavIndex(BuildContext context) {
    final location = GoRouterState.of(context).uri.path;
    if (location.startsWith(Routes.invitations)) return 1;
    if (location.startsWith(Routes.transactions)) return 2;
    if (location.startsWith(Routes.profile)) return 3;
    return 0; // Dashboard is default
  }

  /// Build the appropriate navbar based on whether family is selected
  BottomNavigationBar _buildNavBar(BuildContext context) {
    if (_isFamilySelected) {
      // Show full 5-item navbar
      return BottomNavigationBar(
        currentIndex: _getCurrentNavIndex(context),
        type: BottomNavigationBarType.fixed,
        backgroundColor: Colors.white,
        selectedItemColor: const Color(0xFF5B7CB5),
        unselectedItemColor: const Color(0xFF5B6B8C).withOpacity(0.6),
        onTap: (index) {
          switch (index) {
            case 0:
              context.go(Routes.dashboard);
              break;
            case 1:
              context.go(Routes.invitations);
              break;
            case 2:
              context.go(Routes.transactions);
              break;
            case 3:
              context.go(Routes.myFamily);
              break;
            case 4:
              context.go(Routes.settings);
              break;
          }
        },
        items: const [
          BottomNavigationBarItem(
            icon: Icon(Icons.home),
            label: 'Dashboard',
          ),
          BottomNavigationBarItem(
            icon: Icon(Icons.mail),
            label: 'Invitations',
          ),
          BottomNavigationBarItem(
            icon: Icon(Icons.receipt),
            label: 'Transactions',
          ),
          BottomNavigationBarItem(
            icon: Icon(Icons.people),
            label: 'My Family',
          ),
          BottomNavigationBarItem(
            icon: Icon(Icons.settings),
            label: 'Settings',
          ),
        ],
      );
    } else {
      // Show 2-item navbar for users without family selected
      return BottomNavigationBar(
        currentIndex: 1, // Default to invitations
        type: BottomNavigationBarType.fixed,
        backgroundColor: Colors.white,
        selectedItemColor: const Color(0xFF5B7CB5),
        unselectedItemColor: const Color(0xFF5B6B8C).withOpacity(0.6),
        onTap: (index) {
          switch (index) {
            case 0:
              context.go(Routes.selectFamily);
              break;
            case 1:
              // Already on invitations
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
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    return BlocConsumer<InvitationsCubit, InvitationsState>(
      // Listener: Show snackbars for success/error feedback
      listener: (context, state) {
        if (state is InvitationsError) {
          // Capture cubit reference before showing snackbar to avoid deactivated context
          final cubit = context.read<InvitationsCubit>();
          
          // Show error snackbar with retry button
          ScaffoldMessenger.of(context).showSnackBar(
            SnackBar(
              content: Text(state.message),
              duration: const Duration(seconds: 5),
              action: SnackBarAction(
                label: 'Retry',
                onPressed: () => cubit.loadAll(),
              ),
            ),
          );
        }
      },
      // Builder: Show UI based on state
      builder: (context, state) {
        // Loading state: Show centered progress indicator
        if (state is InvitationsLoading) {
          return Scaffold(
            appBar: AppBar(
              title: const Text('Family Invitations'),
              leading: IconButton(
                icon: const Icon(Icons.arrow_back),
                onPressed: () => context.go(Routes.dashboard),
              ),
            ),
            body: const Center(
              child: CircularProgressIndicator(),
            ),
            bottomNavigationBar: _buildNavBar(context),
          );
        }

        // Error state: Show error message with retry button
        if (state is InvitationsError) {
          return Scaffold(
            appBar: AppBar(
              title: const Text('Family Invitations'),
              leading: IconButton(
                icon: const Icon(Icons.arrow_back),
                onPressed: () => context.go(Routes.dashboard),
              ),
            ),
            body: Center(
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  Icon(
                    Icons.error_outline,
                    size: 64,
                    color: Colors.red[300],
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
                    child: const Text('Retry'),
                  ),
                ],
              ),
            ),
            bottomNavigationBar: _buildNavBar(context),
          );
        }

        // Loaded state: Show full UI with tabs
        if (state is InvitationsLoaded) {
          // For now, using empty string as fallback for currentUserEmail
          // In production, should fetch from localStorage or session
          final currentUserEmail = '';
          // Capture cubit reference for use in callbacks
          final cubit = context.read<InvitationsCubit>();

          // If no family selected, only show Received tab
          if (!_isFamilySelected) {
            return Scaffold(
              appBar: AppBar(
                title: const Text('Family Invitations'),
                leading: IconButton(
                  icon: const Icon(Icons.arrow_back),
                  onPressed: () => context.go(Routes.dashboard),
                ),
              ),
              body: ReceivedInvitationsTab(
                invitations: state.receivedInvitations,
                loadingInvitationId: state.loadingInvitationId,
                onAccept: (id) =>
                    context.read<InvitationsCubit>().acceptInvitation(id),
                onDecline: (id) =>
                    context.read<InvitationsCubit>().declineInvitation(id),
              ),
              bottomNavigationBar: _buildNavBar(context),
            );
          }

          // Family selected: Show both tabs
          return DefaultTabController(
            length: 2,
            initialIndex: state.selectedTab,
            child: Scaffold(
              appBar: AppBar(
                title: const Text('Family Invitations'),
                leading: IconButton(
                  icon: const Icon(Icons.arrow_back),
                  onPressed: () => context.go(Routes.dashboard),
                ),
                bottom: TabBar(
                  onTap: (index) =>
                      context.read<InvitationsCubit>().switchTab(index),
                  tabs: const [
                    Tab(text: 'Received'),
                    Tab(text: 'Sent'),
                  ],
                ),
              ),
              body: TabBarView(
                children: [
                  // Received Invitations Tab (Pending only)
                  ReceivedInvitationsTab(
                    invitations: state.receivedInvitations,
                    loadingInvitationId: state.loadingInvitationId,
                    onAccept: (id) =>
                        context.read<InvitationsCubit>().acceptInvitation(id),
                    onDecline: (id) =>
                        context.read<InvitationsCubit>().declineInvitation(id),
                  ),

                  // Sent Invitations Tab (All statuses, grouped)
                  SentInvitationsTab(
                    invitations: state.sentInvitations,
                    loadingInvitationId: state.loadingInvitationId,
                    onCancel: (id) =>
                        context.read<InvitationsCubit>().cancelInvitation(id),
                  ),
                ],
              ),

              // Floating Action Button: Send new invitation
              floatingActionButton: FloatingActionButton(
                onPressed: () {
                  showDialog(
                    context: context,
                    builder: (dialogContext) => SendInvitationDialog(
                      currentUserEmail: currentUserEmail,
                      cubit: cubit,
                    ),
                  );
                },
                tooltip: 'Send Invitation',
                child: const Icon(Icons.mail_outline),
              ),
              bottomNavigationBar: _buildNavBar(context),
            ),
          );
        }

        // Initial state: Show loading
        return Scaffold(
          appBar: AppBar(
            title: const Text('Family Invitations'),
            leading: IconButton(
              icon: const Icon(Icons.arrow_back),
              onPressed: () => context.go(Routes.dashboard),
            ),
          ),
          body: const Center(
            child: CircularProgressIndicator(),
          ),
          bottomNavigationBar: _buildNavBar(context),
        );
      },
    );
  }
}
