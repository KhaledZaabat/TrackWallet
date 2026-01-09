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
/// Usage:
/// - Place in routes as '/my-family' route
/// - Accessible only after family selection (protected by route guard)
/// - Automatically loads data on page open (via initState)
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
  // Default to true, will check on init

  @override
  void initState() {
    super.initState();
    // Load family data when page opens
    WidgetsBinding.instance.addPostFrameCallback((_) {
      if (mounted) {
        // Data loading logic if any
        context.read<MyFamilyCubit>().loadFamilyDetails();
      }
    });
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
    );
  }
}
