
import 'package:famxpense/core/theme/app_colors.dart';
import 'package:famxpense/l10n/app_localizations.dart';
import 'package:flutter/material.dart';

class AppBottomNavBar extends StatelessWidget {
  final bool isFamilySelected;
  final int currentBranchIndex;
  final Function(int) onBranchTap;

  const AppBottomNavBar({
    super.key,
    required this.isFamilySelected,
    required this.currentBranchIndex,
    required this.onBranchTap,
  });

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context)!;
    
    int navIndex = 0;
    if (isFamilySelected) {
      navIndex = currentBranchIndex;
      if (navIndex > 4) navIndex = 0; 
    } else {
      if (currentBranchIndex == 5) {
        navIndex = 0;
      } else if (currentBranchIndex == 1) {
        navIndex = 1;
      } else if (currentBranchIndex == 6) {
        navIndex = 1;
      } else {
        navIndex = 0;
      }
    }

    if (isFamilySelected) {
      return BottomNavigationBar(
        currentIndex: navIndex,
        type: BottomNavigationBarType.fixed,
        backgroundColor: AppColors.surface,
        selectedItemColor: AppColors.primary,
        unselectedItemColor: AppColors.grey,
        selectedLabelStyle: const TextStyle(fontWeight: FontWeight.w700, fontSize: 12),
        unselectedLabelStyle: const TextStyle(fontWeight: FontWeight.w500, fontSize: 12),
        elevation: 8,
        onTap: (index) {
          onBranchTap(index);
        },
        items: [
          BottomNavigationBarItem(
            icon: const Icon(Icons.dashboard_rounded),
            label: l10n.dashboard,
          ),
          BottomNavigationBarItem(
            icon: const Icon(Icons.mail_rounded),
            label: l10n.invitations,
          ),
          BottomNavigationBarItem(
            icon: const Icon(Icons.receipt_long_rounded),
            label: l10n.transactions,
          ),
          BottomNavigationBarItem(
            icon: const Icon(Icons.people_rounded),
            label: l10n.myFamily,
          ),
          BottomNavigationBarItem(
            icon: const Icon(Icons.settings_rounded),
            label: l10n.settings,
          ),
        ],
      );
    } else {
      return BottomNavigationBar(
        currentIndex: navIndex,
        type: BottomNavigationBarType.fixed,
        backgroundColor: AppColors.surface,
        selectedItemColor: AppColors.primary,
        unselectedItemColor: AppColors.grey,
        elevation: 8,
        onTap: (index) {
          if (index == 0) {
            onBranchTap(5);
          } else {
            onBranchTap(6);
          }
        },
        items: [
          BottomNavigationBarItem(
            icon: const Icon(Icons.people_rounded),
            label: l10n.families,
          ),
          BottomNavigationBarItem(
            icon: const Icon(Icons.mail_rounded),
            label: l10n.invitations,
          ),
        ],
      );
    }
  }
}

