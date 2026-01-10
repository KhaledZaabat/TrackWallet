import 'package:famxpense/core/di/setup_dependency_injection.dart';
import 'package:famxpense/core/services/category_service.dart';
import 'package:famxpense/core/theme/app_colors.dart';
import 'package:famxpense/models/Transactions/transaction_models.dart';
import 'package:flutter/material.dart';
import 'package:intl/intl.dart';

/// Transaction card widget for displaying individual transactions
class TransactionCard extends StatelessWidget {
  final TransactionItem transaction;
  final VoidCallback onTap;

  const TransactionCard({
    super.key,
    required this.transaction,
    required this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    final categoryService = getIt<CategoryService>();
    final category =
        categoryService.getCategoryById(transaction.category.categoryId);
    final isIncome = transaction.isIncome;
    final currency = NumberFormat.simpleCurrency();
    final dateFormat = DateFormat('MMM dd, yyyy');

    return InkWell(
      onTap: onTap,
      borderRadius: BorderRadius.circular(12),
      child: Container(
        padding: const EdgeInsets.all(16),
        decoration: BoxDecoration(
          color: Colors.white,
          borderRadius: BorderRadius.circular(12),
          border: Border.all(
            color: AppColors.border,
            width: 1.5,
          ),
          boxShadow: [
            BoxShadow(
              color: Colors.black.withOpacity(0.03),
              blurRadius: 8,
              offset: const Offset(0, 2),
            ),
          ],
        ),
        child: Row(
          children: [
            _CategoryIcon(
              icon: category?.icon ?? Icons.category_outlined,
              isIncome: isIncome,
            ),
            const SizedBox(width: 16),
            Expanded(
              child: _TransactionDetails(
                title: transaction.title ?? category?.displayName ?? 'Transaction',
                categoryName: category?.displayName ?? transaction.category.name,
                creatorName: transaction.creator.fullName ?? 'Unknown',
                date: dateFormat.format(transaction.transactedOn),
              ),
            ),
            const SizedBox(width: 12),
            _AmountDisplay(
              amount: currency.format(transaction.amount),
              isIncome: isIncome,
            ),
          ],
        ),
      ),
    );
  }
}

class _CategoryIcon extends StatelessWidget {
  final IconData icon;
  final bool isIncome;

  const _CategoryIcon({
    required this.icon,
    required this.isIncome,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      width: 48,
      height: 48,
      decoration: BoxDecoration(
        color: (isIncome ? AppColors.success : AppColors.error).withOpacity(0.1),
        borderRadius: BorderRadius.circular(10),
      ),
      child: Icon(
        icon,
        color: isIncome ? AppColors.success : AppColors.error,
        size: 24,
      ),
    );
  }
}

class _TransactionDetails extends StatelessWidget {
  final String title;
  final String categoryName;
  final String creatorName;
  final String date;

  const _TransactionDetails({
    required this.title,
    required this.categoryName,
    required this.creatorName,
    required this.date,
  });

  @override
  Widget build(BuildContext context) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(
          title,
          style: const TextStyle(
            fontSize: 15,
            fontWeight: FontWeight.w700,
            color: AppColors.textPrimary,
          ),
          maxLines: 1,
          overflow: TextOverflow.ellipsis,
        ),
        const SizedBox(height: 4),
        Row(
          children: [
            Icon(
              Icons.label_outline_rounded,
              size: 14,
              color: AppColors.textSecondary.withOpacity(0.6),
            ),
            const SizedBox(width: 4),
            Flexible(
              child: Text(
                categoryName,
                style: TextStyle(
                  fontSize: 12,
                  color: AppColors.textSecondary.withOpacity(0.6),
                ),
                maxLines: 1,
                overflow: TextOverflow.ellipsis,
              ),
            ),
            const SizedBox(width: 8),
            Icon(
              Icons.person_outline_rounded,
              size: 14,
              color: AppColors.textSecondary.withOpacity(0.6),
            ),
            const SizedBox(width: 4),
            Flexible(
              child: Text(
                creatorName,
                style: TextStyle(
                  fontSize: 12,
                  color: AppColors.textSecondary.withOpacity(0.6),
                ),
                maxLines: 1,
                overflow: TextOverflow.ellipsis,
              ),
            ),
          ],
        ),
        const SizedBox(height: 4),
        Row(
          children: [
            Icon(
              Icons.calendar_today_outlined,
              size: 14,
              color: AppColors.textSecondary.withOpacity(0.5),
            ),
            const SizedBox(width: 4),
            Text(
              date,
              style: TextStyle(
                fontSize: 11,
                color: AppColors.textSecondary,
              ),
            ),
          ],
        ),
      ],
    );
  }
}

class _AmountDisplay extends StatelessWidget {
  final String amount;
  final bool isIncome;

  const _AmountDisplay({
    required this.amount,
    required this.isIncome,
  });

  @override
  Widget build(BuildContext context) {
    return Row(
      mainAxisSize: MainAxisSize.min,
      children: [
        Icon(
          isIncome ? Icons.arrow_upward_rounded : Icons.arrow_downward_rounded,
          size: 16,
          color: isIncome ? AppColors.success : AppColors.error,
        ),
        const SizedBox(width: 4),
        Text(
          amount,
          style: TextStyle(
            fontSize: 16,
            fontWeight: FontWeight.w700,
            color: isIncome ? AppColors.success : AppColors.error,
          ),
        ),
      ],
    );
  }
}
