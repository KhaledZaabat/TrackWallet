
import 'package:famxpense/core/theme/app_colors.dart';
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
    // Mapping Logic:
    // Branches:
    // 0: Dashboard
    // 1: Invitations
    // 2: Transactions
    // 3: MyFamily
    // 4: Settings
    // 5: SelectFamily

    int navIndex = 0;
    if (isFamilySelected) {
      // Family Mode (5 items):
      // 0->0 (Dashboard)
      // 1->1 (Invitations)
      // 2->2 (Transactions)
      // 3->3 (MyFamily)
      // 4->4 (Settings)
      navIndex = currentBranchIndex;
      // Safety check if we somehow ended up on SelectFamily (5) while "Family Selected" is true
      if (navIndex > 4) navIndex = 0; 
    } else {
      // No Family Mode (3 items):
      // 0->5 (SelectFamily -> Families tab)
      // 1->6 (Guest Invitations -> Invitations tab)
      // 2->4 (Settings -> Settings tab)
      if (currentBranchIndex == 5) {
        navIndex = 0; // Families tab
      } else if (currentBranchIndex == 1 || currentBranchIndex == 6) {
        navIndex = 1; // Invitations tab (Guest)
      } else if (currentBranchIndex == 4) {
        navIndex = 2; // Settings tab
      } else {
        navIndex = 0; // Default
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
        items: const [
          BottomNavigationBarItem(
            icon: Icon(Icons.dashboard_rounded),
            label: 'Dashboard',
          ),
          BottomNavigationBarItem(
            icon: Icon(Icons.mail_rounded),
            label: 'Invitations',
          ),
          BottomNavigationBarItem(
            icon: Icon(Icons.receipt_long_rounded),
            label: 'Transactions',
          ),
          BottomNavigationBarItem(
            icon: Icon(Icons.people_rounded),
            label: 'My Family',
          ),
          BottomNavigationBarItem(
            icon: Icon(Icons.settings_rounded),
            label: 'Settings',
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
        selectedLabelStyle: const TextStyle(fontWeight: FontWeight.w700, fontSize: 12),
        unselectedLabelStyle: const TextStyle(fontWeight: FontWeight.w500, fontSize: 12),
        elevation: 8,
        onTap: (index) {
          if (index == 0) {
            onBranchTap(5); // Go to Select Family
          } else if (index == 1) {
            onBranchTap(6); // Go to Guest Invitations
          } else {
            onBranchTap(4); // Go to Settings
          }
        },
        items: const [
          BottomNavigationBarItem(
            icon: Icon(Icons.people_rounded),
            label: 'Families',
          ),
          BottomNavigationBarItem(
            icon: Icon(Icons.mail_rounded),
            label: 'Invitations',
          ),
          BottomNavigationBarItem(
            icon: Icon(Icons.settings_rounded),
            label: 'Settings',
          ),
        ],
      );
    }
  }
}

