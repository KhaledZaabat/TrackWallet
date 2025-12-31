import 'package:famxpense/common/widgets/app_bar.dart';
import 'package:famxpense/core/configs/theme/app_colors.dart';
import 'package:famxpense/core/di/service_locator.dart';
import 'package:famxpense/presentation/family/cubit/manage_family_cubit.dart';
import 'package:famxpense/presentation/family/cubit/manage_family_state.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:go_router/go_router.dart';

class ManageFamilyUsersPage extends StatelessWidget {
  final String familyId;
  const ManageFamilyUsersPage({super.key, required this.familyId});

  @override
  Widget build(BuildContext context) {
    return BlocProvider(
      create: (_) => sl<ManageFamilyCubit>()..load(familyId),
      child: HeroControllerScope.none(
        child: Scaffold(
          backgroundColor: const Color(0xFFF5F8FA),
          body: BlocBuilder<ManageFamilyCubit, ManageFamilyState>(
            builder: (context, state) {
              if (state.isLoading) {
                return const Center(child: CircularProgressIndicator());
              }
              if (state.error != null) {
                return Center(
                  child: Column(
                    mainAxisSize: MainAxisSize.min,
                    children: [
                      Text(
                        state.error!,
                        style: const TextStyle(
                          color: Colors.redAccent,
                          fontWeight: FontWeight.w700,
                        ),
                      ),
                      const SizedBox(height: 12),
                      TextButton(
                        onPressed: () => context.pop(),
                        child: const Text('Go back'),
                      )
                    ],
                  ),
                );
              }

              return CustomScrollView(
                slivers: [
                  MyAppBar(
                    title: 'Manage Users',
                    leading: const Icon(Icons.arrow_back),
                    leadingOnPressed: () => context.pop(),
                  ),
                  SliverToBoxAdapter(
                    child: Padding(
                      padding: const EdgeInsets.symmetric(
                          horizontal: 18, vertical: 12),
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          const Text(
                            'Members & Roles',
                            style: TextStyle(
                              color: AppColors.mainBlackShade,
                              fontWeight: FontWeight.w800,
                              fontSize: 16,
                            ),
                          ),
                          const SizedBox(height: 12),
                          ...state.members.map(
                            (m) => Container(
                              margin: const EdgeInsets.only(bottom: 10),
                              padding: const EdgeInsets.symmetric(
                                  horizontal: 12, vertical: 10),
                              decoration: BoxDecoration(
                                color: Colors.white,
                                borderRadius: BorderRadius.circular(10),
                                border: Border.all(
                                    color: AppColors.stroke, width: 1),
                              ),
                              child: Row(
                                children: [
                                  CircleAvatar(
                                    radius: 22,
                                    backgroundColor: AppColors.primary
                                        .withValues(alpha: 0.12),
                                    backgroundImage: (m.avatarUrl != null &&
                                            m.avatarUrl!.isNotEmpty)
                                        ? NetworkImage(m.avatarUrl!)
                                        : null,
                                    child: (m.avatarUrl == null ||
                                            m.avatarUrl!.isEmpty)
                                        ? Text(
                                            _initials(m.name),
                                            style: const TextStyle(
                                              color: AppColors.primary,
                                              fontWeight: FontWeight.w800,
                                            ),
                                          )
                                        : null,
                                  ),
                                  const SizedBox(width: 12),
                                  Expanded(
                                    child: Column(
                                      crossAxisAlignment:
                                          CrossAxisAlignment.start,
                                      children: [
                                        Text(
                                          m.name,
                                          style: const TextStyle(
                                            fontWeight: FontWeight.w800,
                                            fontSize: 14,
                                            color: AppColors.mainBlackShade,
                                          ),
                                        ),
                                        const SizedBox(height: 4),
                                        Text(
                                          m.isParent ? 'Parent' : 'Member',
                                          style: TextStyle(
                                            color: m.isParent
                                                ? AppColors.primary
                                                : AppColors.mainGrayShade,
                                            fontWeight: FontWeight.w700,
                                            fontSize: 12,
                                          ),
                                        ),
                                      ],
                                    ),
                                  ),
                                  TextButton(
                                    onPressed: m.isParent ? null : () {},
                                    child: const Text('Promote'),
                                  ),
                                  TextButton(
                                    onPressed: () {},
                                    child: const Text('Remove'),
                                  ),
                                ],
                              ),
                            ),
                          ),
                          const SizedBox(height: 16),
                          if (state.error != null)
                            Padding(
                              padding: const EdgeInsets.only(bottom: 8),
                              child: Text(
                                state.error!,
                                style: const TextStyle(
                                  color: Colors.redAccent,
                                  fontWeight: FontWeight.w700,
                                ),
                              ),
                            ),
                          SizedBox(
                            width: double.infinity,
                            child: OutlinedButton.icon(
                              onPressed: () => _showInviteSheet(context),
                              icon: const Icon(Icons.email_outlined),
                              label: const Text('Invite by email'),
                              style: OutlinedButton.styleFrom(
                                foregroundColor: AppColors.primary,
                                side: const BorderSide(
                                    color: AppColors.primary, width: 1.2),
                                padding:
                                    const EdgeInsets.symmetric(vertical: 12),
                                shape: RoundedRectangleBorder(
                                  borderRadius: BorderRadius.circular(10),
                                ),
                              ),
                            ),
                          ),
                        ],
                      ),
                    ),
                  ),
                ],
              );
            },
          ),
        ),
      ),
    );
  }

  String _initials(String name) {
    final parts = name.trim().split(' ');
    if (parts.length == 1) return name.substring(0, 1).toUpperCase();
    return (parts[0].substring(0, 1) + parts[1].substring(0, 1)).toUpperCase();
  }

  void _showInviteSheet(BuildContext context) {
    final controller = TextEditingController();
    bool isParent = false;

    showModalBottomSheet(
      context: context,
      showDragHandle: true,
      shape: const RoundedRectangleBorder(
        borderRadius: BorderRadius.vertical(top: Radius.circular(14)),
      ),
      builder: (ctx) {
        return StatefulBuilder(
          builder: (ctx, setState) {
            return Padding(
              padding: const EdgeInsets.all(16),
              child: Column(
                mainAxisSize: MainAxisSize.min,
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  const Text(
                    'Invite by email',
                    style: TextStyle(
                      fontWeight: FontWeight.w800,
                      fontSize: 16,
                      color: AppColors.mainBlackShade,
                    ),
                  ),
                  const SizedBox(height: 12),
                  TextField(
                    controller: controller,
                    keyboardType: TextInputType.emailAddress,
                    decoration: const InputDecoration(
                      hintText: 'email@example.com',
                      border: OutlineInputBorder(),
                    ),
                  ),
                  const SizedBox(height: 12),
                  Row(
                    children: [
                      Checkbox(
                        value: isParent,
                        onChanged: (value) {
                          setState(() {
                            isParent = value ?? false;
                          });
                        },
                      ),
                      const Text('Make parent'),
                    ],
                  ),
                  const SizedBox(height: 12),
                  SizedBox(
                    width: double.infinity,
                    child: ElevatedButton(
                      onPressed: () {
                        Navigator.of(ctx).pop();
                        context.read<ManageFamilyCubit>().inviteByEmail(
                              controller.text,
                              isParent: isParent,
                            );
                      },
                      style: ElevatedButton.styleFrom(
                        backgroundColor: AppColors.primary,
                        shape: RoundedRectangleBorder(
                          borderRadius: BorderRadius.circular(10),
                        ),
                      ),
                      child: const Text(
                        'Send invite',
                        style: TextStyle(
                          color: Colors.white,
                          fontWeight: FontWeight.w700,
                        ),
                      ),
                    ),
                  ),
                ],
              ),
            );
          },
        );
      },
    );
  }
}
