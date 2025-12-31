import 'package:famxpense/common/widgets/app_bar.dart';
import 'package:famxpense/core/configs/theme/app_colors.dart';
import 'package:famxpense/core/router/routes.dart';
import 'package:famxpense/data/database/repositories/abstractions/i_auth_repository.dart';
import 'package:famxpense/core/di/service_locator.dart';
import 'package:famxpense/presentation/family/cubit/family_cubit.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:go_router/go_router.dart';

class MorePage extends StatelessWidget {
  const MorePage({super.key});

  Future<void> _logout(BuildContext context) async {
    await sl<IAuthRepository>().logout(); // clears SessionRepository

    if (!context.mounted) {
      return;
    }

    // reset in-memory family state
    context.read<FamilyCubit>().resetOnLogout();

    // navigate to login
    context.go('/login'); // or context.go(Routes.login) if you use that
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFFF5F8FA),
      body: CustomScrollView(
        slivers: <Widget>[
          const MyAppBar(
            title: 'More Actions',
            collapsedBackgroundColor: Color(0xFFE4EAF5),
            enableShadow: false,
          ),
          SliverToBoxAdapter(
            child: Padding(
              padding: const EdgeInsets.symmetric(horizontal: 10.0),
              child: Column(
                children: <Widget>[
                  const SizedBox(height: 20),
                  SettingsTile(
                    icon: Icons.settings,
                    title: 'Settings & Customization',
                    subtitle: 'Theme, Notifications, Import/Export',
                    onTap: () {
                      context.push(Routes.settings);
                    },
                  ),
                  GridView.count(
                    physics: const NeverScrollableScrollPhysics(),
                    crossAxisCount: 2,
                    mainAxisSpacing: 12,
                    crossAxisSpacing: 12,
                    shrinkWrap: true,
                    childAspectRatio: 2.8,
                    children: <Widget>[
                      QuickActionTile(
                        icon: Icons.error_outline,
                        label: 'My Info',
                        onTap: () => context.push(Routes.settings),
                      ),
                      QuickActionTile(
                        icon: Icons.edit_note,
                        label: 'My Invites',
                        onTap: () => context.push(Routes.myInvites),
                      ),
                      const QuickActionTile(
                        icon: Icons.notifications_none,
                        label: 'Notifications',
                      ),
                      QuickActionTile(
                        icon: Icons.logout,
                        label: 'Logout',
                        onTap: () => _logout(context),
                      ),
                    ],
                  ),
                ],
              ),
            ),
          ),
          const SliverToBoxAdapter(
            child: SizedBox(
              height: 470,
            ),
          ),
        ],
      ),
    );
  }
}

class SettingsTile extends StatelessWidget {
  const SettingsTile({
    super.key,
    required this.icon,
    required this.title,
    required this.subtitle,
    this.onTap,
  });

  final IconData icon;
  final String title;
  final String subtitle;
  final VoidCallback? onTap;

  @override
  Widget build(BuildContext context) {
    return InkWell(
      borderRadius: BorderRadius.circular(10),
      onTap: onTap,
      child: Container(
        padding: const EdgeInsets.symmetric(
          horizontal: 16,
          vertical: 12,
        ),
        decoration: BoxDecoration(
          color: Colors.white,
          borderRadius: BorderRadius.circular(12),
          border: Border.all(
            width: 1.6,
            color: AppColors.stroke,
          ),
          boxShadow: <BoxShadow>[
            BoxShadow(
              color: Colors.black.withValues(alpha: 0.03),
              blurRadius: 12,
              offset: const Offset(0, 6),
            ),
          ],
        ),
        child: Row(
          children: <Widget>[
            Icon(
              icon,
              size: 28,
              color: AppColors.mainGrayShade,
            ),
            const SizedBox(width: 12),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: <Widget>[
                  Text(
                    title,
                    style: const TextStyle(
                      fontSize: 16,
                      fontWeight: FontWeight.w700,
                      color: AppColors.mainBlackShade,
                    ),
                  ),
                  const SizedBox(height: 4),
                  Text(
                    subtitle,
                    style: TextStyle(
                      fontSize: 12.5,
                      fontWeight: FontWeight.w500,
                      color: AppColors.mainGrayShade,
                    ),
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
}

class QuickActionTile extends StatelessWidget {
  const QuickActionTile({
    super.key,
    required this.icon,
    required this.label,
    this.onTap,
  });

  final IconData icon;
  final String label;
  final VoidCallback? onTap;

  @override
  Widget build(BuildContext context) {
    return InkWell(
      borderRadius: BorderRadius.circular(10),
      onTap: onTap,
      child: Container(
        padding: const EdgeInsets.symmetric(
          horizontal: 16,
          vertical: 14,
        ),
        decoration: BoxDecoration(
          color: Colors.white,
          borderRadius: BorderRadius.circular(12),
          border: Border.all(
            color: AppColors.stroke,
            width: 1.6,
          ),
        ),
        child: Row(
          children: <Widget>[
            Icon(
              icon,
              size: 22,
              color: AppColors.mainGrayShade,
            ),
            const SizedBox(width: 12),
            Expanded(
              child: Text(
                label,
                style: const TextStyle(
                  fontSize: 15,
                  fontWeight: FontWeight.w700,
                  color: AppColors.mainBlackShade,
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }
}
