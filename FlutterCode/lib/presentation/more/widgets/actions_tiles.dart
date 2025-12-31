import 'package:famxpense/core/configs/theme/app_colors.dart';
import 'package:flutter/material.dart';

class ActionsTiles extends StatelessWidget {
  const ActionsTiles(
      {super.key,
      required this.icon,
      required this.title,
      this.additionalAction});

  final IconData icon;
  final String title;
  final Widget? additionalAction;

  @override
  Widget build(BuildContext context) {
    return Row(
      children: [
        Icon(
          icon,
          size: 26,
          color: AppColors.mainGrayShade,
        ),
        const SizedBox(width: 18),
        Expanded(
          child: Text(
            title,
            style: TextStyle(
              fontSize: 16,
              fontWeight: FontWeight.w700,
              color: AppColors.mainBlackShade,
            ),
          ),
        ),
        if (additionalAction != null) additionalAction!,
      ],
    );
  }
}
