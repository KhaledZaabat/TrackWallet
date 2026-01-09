import 'package:famxpense/common/widgets/app_bottom_nav_bar.dart';
import 'package:famxpense/core/di/setup_dependency_injection.dart';
import 'package:famxpense/core/storage/local_storage.dart';
import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';

class ScaffoldWithNestedNavigation extends StatefulWidget {
  const ScaffoldWithNestedNavigation({
    Key? key,
    required this.navigationShell,
  }) : super(key: key);

  final StatefulNavigationShell navigationShell;

  @override
  State<ScaffoldWithNestedNavigation> createState() =>
      _ScaffoldWithNestedNavigationState();
}

class _ScaffoldWithNestedNavigationState
    extends State<ScaffoldWithNestedNavigation> {
  bool _isFamilySelected = false;

  @override
  void initState() {
    super.initState();
    _checkFamilySelection();
  }

  Future<void> _checkFamilySelection() async {
    final familyId = await getIt<LocalStorage>().getSelectedFamilyId();
    if (mounted) {
      setState(() {
        _isFamilySelected = familyId != null && familyId.isNotEmpty;
      });
    }
  }

  // We might want to re-check when navigating, in case family state changes
  // For now, simpler approach. Real app might use a Stream or Cubit for this global state.
  // Since we are doing a major refactor, let's keep using the LocalStorage check for consistency with existing pages.
  // Note: If family changes, the router usually redirects anyway, but the UI mode (2 tabs vs 5 tabs) needs to update.

  void _goBranch(int index) {
    widget.navigationShell.goBranch(
      index,
      // A common pattern when using bottom navigation bars is to support
      // navigating to the initial location when tapping the item that is
      // already active. This example demonstrates how to support this behavior,
      // using the initialLocation parameter of goBranch.
      initialLocation: index == widget.navigationShell.currentIndex,
    );
  }

  @override
  Widget build(BuildContext context) {
    // Determine effective family mode based on the functionality of the current branch
    // Branches 0 (Dashboard), 2 (Transactions), 3 (MyFamily), 4 (Settings) are ONLY for Family mode
    // Branch 5 (SelectFamily) is ONLY for No-Family mode
    // Branch 1 (Invitations) is shared, so we subscribe to the data source (_isFamilySelected)
    
    bool effectiveIsFamilySelected = _isFamilySelected;
    final currentIndex = widget.navigationShell.currentIndex;
    
    // Branch 5 (SelectFamily) and Branch 6 (InvitationsGuest) are No-Family mode
    if (currentIndex == 5 || currentIndex == 6) {
      effectiveIsFamilySelected = false;
    } else if (currentIndex == 0 || currentIndex == 2 || currentIndex == 3 || currentIndex == 4 || currentIndex == 1) {
      effectiveIsFamilySelected = true;
    }

    return Scaffold(
      body: widget.navigationShell,
      bottomNavigationBar: AppBottomNavBar(
        isFamilySelected: effectiveIsFamilySelected,
        currentBranchIndex: currentIndex,
        onBranchTap: _goBranch,
      ),
    );
  }
}
