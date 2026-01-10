import 'package:famxpense/core/router/routes.dart';
import 'package:famxpense/core/theme/app_colors.dart';
import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:intl/intl.dart';

/// Dashboard header widget with user greeting and family card
class DashboardHeader extends StatelessWidget {
  final String fullName;
  final String? profileImageUrl;
  final String familyName;
  final double? currentBudget;

  const DashboardHeader({
    super.key,
    required this.fullName,
    this.profileImageUrl,
    required this.familyName,
    this.currentBudget,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      decoration: const BoxDecoration(
        gradient: AppColors.wolfGradient,
      ),
      child: SafeArea(
        child: Padding(
          padding: const EdgeInsets.fromLTRB(20, 20, 20, 32),
          child: Column(
            mainAxisAlignment: MainAxisAlignment.end,
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              _UserGreeting(
                fullName: fullName,
                profileImageUrl: profileImageUrl,
              ),
              const SizedBox(height: 24),
              FamilySwitchCard(
                familyName: familyName,
                currentBudget: currentBudget,
              ),
            ],
          ),
        ),
      ),
    );
  }
}

/// User greeting section with avatar and name
class _UserGreeting extends StatelessWidget {
  final String fullName;
  final String? profileImageUrl;

  const _UserGreeting({
    required this.fullName,
    this.profileImageUrl,
  });

  @override
  Widget build(BuildContext context) {
    return Row(
      children: [
        GestureDetector(
          onTap: () => context.push(Routes.profile),
          child: CircleAvatar(
            radius: 28,
            backgroundColor: Colors.white.withOpacity(0.2),
            backgroundImage: profileImageUrl != null
                ? NetworkImage(profileImageUrl!)
                : null,
            child: profileImageUrl == null
                ? Text(
                    fullName.isNotEmpty
                        ? fullName.substring(0, 1).toUpperCase()
                        : '?',
                    style: const TextStyle(
                      fontSize: 24,
                      fontWeight: FontWeight.w700,
                      color: Colors.white,
                    ),
                  )
                : null,
          ),
        ),
        const SizedBox(width: 16),
        Expanded(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                'Welcome back,',
                style: TextStyle(
                  fontSize: 14,
                  color: Colors.white.withOpacity(0.9),
                ),
              ),
              const SizedBox(height: 4),
              Text(
                fullName,
                style: const TextStyle(
                  fontSize: 20,
                  fontWeight: FontWeight.w700,
                  color: Colors.white,
                ),
                overflow: TextOverflow.ellipsis,
              ),
            ],
          ),
        ),
      ],
    );
  }
}

/// Clickable family card that navigates to family selection
class FamilySwitchCard extends StatelessWidget {
  final String familyName;
  final double? currentBudget;

  const FamilySwitchCard({
    super.key,
    required this.familyName,
    this.currentBudget,
  });

  @override
  Widget build(BuildContext context) {
    final currency = NumberFormat.simpleCurrency();

    return Material(
      color: Colors.transparent,
      child: InkWell(
        onTap: () => context.go(Routes.selectFamily),
        borderRadius: BorderRadius.circular(12),
        splashColor: Colors.white.withOpacity(0.1),
        highlightColor: Colors.white.withOpacity(0.05),
        child: Container(
          padding: const EdgeInsets.all(16),
          decoration: BoxDecoration(
            color: Colors.white.withOpacity(0.15),
            borderRadius: BorderRadius.circular(12),
            border: Border.all(
              color: Colors.white.withOpacity(0.2),
              width: 1,
            ),
          ),
          child: Row(
            children: [
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      familyName,
                      style: const TextStyle(
                        fontSize: 16,
                        fontWeight: FontWeight.w600,
                        color: Colors.white,
                      ),
                      overflow: TextOverflow.ellipsis,
                    ),
                    if (currentBudget != null) ...[
                      const SizedBox(height: 6),
                      Text(
                        currency.format(currentBudget),
                        style: const TextStyle(
                          fontSize: 20,
                          fontWeight: FontWeight.w700,
                          color: Colors.white,
                        ),
                      ),
                    ],
                  ],
                ),
              ),
              const Icon(
                Icons.swap_horiz,
                color: Colors.white,
                size: 24,
              ),
            ],
          ),
        ),
      ),
    );
  }
}
