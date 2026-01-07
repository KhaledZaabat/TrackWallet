import 'package:famxpense/core/configs/theme/app_colors.dart';
import 'package:famxpense/core/router/routes.dart';
import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';

class AddTransactionFloatingActionButton extends StatelessWidget {
  const AddTransactionFloatingActionButton({
    super.key,
  });

  @override
  Widget build(BuildContext context) {
    return FloatingActionButton(
      onPressed: () => context.push(Routes.transactionsAdd),
      child: const Icon(Icons.add),
      backgroundColor: AppColors.floatingActions,
      foregroundColor: AppColors.backgroundColor,
    );
  }
}
