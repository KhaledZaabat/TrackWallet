
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
      // No Family Mode (2 items):
      // 0->5 (SelectFamily -> Families tab)
      // 1->1 (Invitations -> Invitations tab)
      if (currentBranchIndex == 5) {
        navIndex = 0; // Families tab
      } else if (currentBranchIndex == 1) {
        navIndex = 1; // Invitations tab
      } else if (currentBranchIndex == 6) {
        navIndex = 1; // Invitations tab (Guest)
      } else {
        navIndex = 0; // Default
      }
    }

    if (isFamilySelected) {
      return BottomNavigationBar(
        currentIndex: navIndex,
        type: BottomNavigationBarType.fixed,
        backgroundColor: Colors.white,
        selectedItemColor: const Color(0xFF5B7CB5),
        unselectedItemColor: const Color(0xFF5B6B8C).withValues(alpha: 0.6),
        onTap: (index) {
          onBranchTap(index);
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
      return BottomNavigationBar(
        currentIndex: navIndex,
        type: BottomNavigationBarType.fixed,
        backgroundColor: Colors.white,
        selectedItemColor: const Color(0xFF5B7CB5),
        unselectedItemColor: const Color(0xFF5B6B8C).withValues(alpha: 0.6),
        onTap: (index) {
          if (index == 0) {
            onBranchTap(5); // Go to Select Family
          } else {
            onBranchTap(6); // Go to Guest Invitations
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

