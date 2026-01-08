import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:go_router/go_router.dart';

import 'package:famxpense/core/di/setup_dependency_injection.dart';
import 'package:famxpense/core/router/routes.dart';
import 'package:famxpense/core/storage/local_storage.dart';
import 'package:famxpense/features/MyFamily/cubit/my_family_cubit.dart';
import 'package:famxpense/features/MyFamily/cubit/my_family_state.dart';
import 'package:famxpense/features/MyFamily/widgets/family_header.dart';
import 'package:famxpense/features/MyFamily/widgets/members_list.dart';

/// MyFamily page displays all members of the currently selected family
///
/// Features:
/// - Family header with name, budget, and bio
/// - List of all family members with profile information
/// - Loading state with spinner
/// - Error handling with retry button
/// - Conditional navbar based on family selection (2-item vs 5-item)
///
/// State Management:
/// - MyFamilyInitial: Initial state before loading
/// - MyFamilyLoading: Loading state during API call
/// - MyFamilyLoaded: Success state with FamilyDetails
/// - MyFamilyError: Error state with message
///
/// Usage:
/// - Place in routes as '/my-family' route
/// - Accessible only after family selection (protected by route guard)
/// - Automatically loads data on page open (via initState)
class MyFamilyPage extends StatefulWidget {
  const MyFamilyPage({super.key});

  @override
  State<MyFamilyPage> createState() => _MyFamilyPageState();
}

class _MyFamilyPageState extends State<MyFamilyPage> {
  bool _isFamilySelected = true; // Default to true, will check on init

  @override
  void initState() {
    super.initState();
    // Load family details after first frame builds
    WidgetsBinding.instance.addPostFrameCallback((_) {
      if (mounted) {
        // Check if family is selected
        getIt<LocalStorage>().getSelectedFamilyId().then((familyId) {
          setState(() {
            _isFamilySelected = familyId != null && familyId.isNotEmpty;
          });
        });
        // Load family details via cubit
        context.read<MyFamilyCubit>().loadFamilyDetails();
      }
    });
  }

  /// Determine which navbar item should be selected based on current route
  int _getCurrentNavIndex(BuildContext context) {
    final location = GoRouterState.of(context).uri.path;
    if (location.startsWith(Routes.myFamily)) return 3;
    if (location.startsWith(Routes.transactions)) return 2;
    if (location.startsWith(Routes.invitations)) return 1;
    if (location.startsWith(Routes.settings)) return 4;
    return 0; // Dashboard is default
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Family Members'),
        centerTitle: false,
        backgroundColor: Colors.white,
        elevation: 0,
        surfaceTintColor: Colors.transparent,
        foregroundColor: Colors.black87,
      ),
      body: BlocConsumer<MyFamilyCubit, MyFamilyState>(
        listener: (context, state) {
          if (state is MyFamilyError) {
            ScaffoldMessenger.of(context).showSnackBar(
              SnackBar(
                content: Text(state.message),
                backgroundColor: Colors.red[400],
                duration: const Duration(seconds: 3),
              ),
            );
          }
        },
        builder: (context, state) {
          if (state is MyFamilyLoading) {
            return const Center(
              child: CircularProgressIndicator(),
            );
          }

          if (state is MyFamilyLoaded) {
            return SingleChildScrollView(
              physics: const AlwaysScrollableScrollPhysics(),
              child: Column(
                children: [
                  FamilyHeader(familyDetails: state.familyDetails),
                  const SizedBox(height: 8),
                  MembersListWidget(members: state.familyDetails.members),
                  const SizedBox(height: 80),
                ],
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
                      Icons.error_outline,
                      size: 64,
                      color: Colors.red[300],
                    ),
                    const SizedBox(height: 16),
                    Text(
                      'Failed to load family',
                      style: Theme.of(context).textTheme.titleMedium?.copyWith(
                            fontWeight: FontWeight.w600,
                          ),
                    ),
                    const SizedBox(height: 8),
                    Text(
                      state.message,
                      textAlign: TextAlign.center,
                      style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                            color: Colors.grey[600],
                          ),
                    ),
                    const SizedBox(height: 24),
                    ElevatedButton.icon(
                      onPressed: () {
                        context.read<MyFamilyCubit>().loadFamilyDetails();
                      },
                      style: ElevatedButton.styleFrom(
                        backgroundColor: Colors.blue[600],
                        foregroundColor: Colors.white,
                        padding: const EdgeInsets.symmetric(
                          horizontal: 24,
                          vertical: 12,
                        ),
                      ),
                      icon: const Icon(Icons.refresh),
                      label: const Text('Retry'),
                    ),
                  ],
                ),
              ),
            );
          }

          return const SizedBox.shrink();
        },
      ),
      bottomNavigationBar: _buildNavBar(context),
    );
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
              context.go(Routes.invitations);
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
}
