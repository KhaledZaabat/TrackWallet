import 'package:famxpense/core/configs/theme/app_colors.dart';
import 'package:famxpense/core/router/destination.dart';
import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';

class LayoutScaffold extends StatelessWidget {
  const LayoutScaffold(
      {Key? key, required this.navigationShell})
      : super(key: key ?? const Key('LayoutScaffold'));

  final StatefulNavigationShell navigationShell;

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: navigationShell,
      bottomNavigationBar: Container(
        decoration: BoxDecoration(
          color: Colors.transparent,
          boxShadow: [
            BoxShadow(
              color: Colors.black
                  // ignore: deprecated_member_use
                  .withValues(alpha: 0.2),
              blurRadius: 10,
              spreadRadius: 0,
              offset: const Offset(0, -4),
            ),
          ],
        ),
        child: NavigationBar(
          indicatorShape: ShapeBorder.lerp(
            RoundedRectangleBorder(
              borderRadius: BorderRadius.circular(10),
            ),
            RoundedRectangleBorder(
              borderRadius: BorderRadius.circular(10),
            ),
            0,
          ),
          labelTextStyle:
              WidgetStateTextStyle.resolveWith((states) {
            if (states.contains(WidgetState.selected)) {
              return const TextStyle(
                fontWeight: FontWeight.bold,
                color: AppColors.mainBlackShade,
              );
            }
            return const TextStyle(
              fontWeight: FontWeight.w400,
              color: AppColors.mainBlackShade,
            );
          }),
          shadowColor:
              // ignore: deprecated_member_use
              Colors.black.withValues(alpha: 0.25),
          elevation: 4,
          height: 79,
          selectedIndex: navigationShell.currentIndex,
          onDestinationSelected: navigationShell.goBranch,
          indicatorColor: AppColors.primary,
          backgroundColor: AppColors.secondary,
          destinations: destinations
              .map((destination) => NavigationDestination(
                    icon: Icon(
                      destination.icon,
                      color: AppColors.mainBlackShade,
                    ),
                    label: destination.label,
                  ))
              .toList(),
        ),
      ),
    );
  }
}

