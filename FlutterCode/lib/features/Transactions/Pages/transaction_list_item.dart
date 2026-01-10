import 'package:famxpense/core/di/setup_dependency_injection.dart';
import 'package:famxpense/core/services/category_service.dart';
import 'package:famxpense/models/Transactions/transaction_models.dart';
import 'package:famxpense/core/theme/app_colors.dart';
import 'package:famxpense/l10n/app_localizations.dart';
import 'package:flutter/material.dart';
import 'package:intl/intl.dart';

class TransactionListItem extends StatelessWidget {
  final TransactionItem transaction;
  final VoidCallback onTap;
  final VoidCallback onLongPress;

  const TransactionListItem({
    super.key,
    required this.transaction,
    required this.onTap,
    required this.onLongPress,
  });

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context)!;
    final categoryService = getIt<CategoryService>();
    final category =
        categoryService.getCategoryById(transaction.category.categoryId);
    final isIncome = transaction.type == TransactionType.Income;

    return InkWell(
      onTap: onTap,
      onLongPress: onLongPress,
      borderRadius: BorderRadius.circular(16),
      child: Container(
        padding: const EdgeInsets.all(16),
        decoration: BoxDecoration(
          color: AppColors.surface,
          borderRadius: BorderRadius.circular(16),
          border: Border.all(
            color: AppColors.border,
            width: 1.5,
          ),
          boxShadow: [
            BoxShadow(
              color: Colors.black.withOpacity(0.03),
              blurRadius: 10,
              offset: const Offset(0, 4),
            ),
          ],
        ),
        child: Row(
          children: [
            _buildIcon(category?.icon, isIncome),
            const SizedBox(width: 14),
            Expanded(child: _buildTransactionInfo(l10n)),
            const SizedBox(width: 12),
            _buildAmount(l10n, isIncome),
          ],
        ),
      ),
    );
  }

  Widget _buildIcon(IconData? icon, bool isIncome) {
    return Container(
      width: 48,
      height: 48,
      decoration: BoxDecoration(
        color: isIncome
            ? AppColors.success.withOpacity(0.15)
            : AppColors.error.withOpacity(0.15),
        shape: BoxShape.circle,
      ),
      child: Icon(
        icon ?? Icons.category_outlined,
        color: isIncome ? AppColors.success : AppColors.error,
        size: 24,
      ),
    );
  }

  Widget _buildTransactionInfo(AppLocalizations l10n) {
    final categoryService = getIt<CategoryService>();
    final category =
        categoryService.getCategoryById(transaction.category.categoryId);

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(
          transaction.title ?? category?.displayName ?? l10n.transaction,
          style: const TextStyle(
            fontWeight: FontWeight.w700,
            fontSize: 15,
            color: AppColors.textPrimary,
          ),
          maxLines: 1,
          overflow: TextOverflow.ellipsis,
        ),
        const SizedBox(height: 4),
        Row(
          children: [
            Icon(
              Icons.access_time_rounded,
              size: 14,
              color: AppColors.textSecondary,
            ),
            const SizedBox(width: 4),
            Text(
              DateFormat('h:mm a').format(transaction.createdAtUtc.toLocal()),
              style: const TextStyle(
                fontSize: 12,
                fontWeight: FontWeight.w600,
                color: AppColors.textSecondary,
              ),
            ),
            if (transaction.notes != null && transaction.notes!.isNotEmpty) ...[
              const SizedBox(width: 8),
              const Icon(
                Icons.note_outlined,
                size: 14,
                color: AppColors.textSecondary,
              ),
            ],
          ],
        ),
        if (category?.displayName != null && transaction.title != null) ...[
          const SizedBox(height: 2),
          Row(
            children: [
              Icon(
                Icons.label_outline_rounded,
                size: 14,
                color: AppColors.textSecondary,
              ),
              const SizedBox(width: 4),
              Flexible(
                child: Text(
                  category!.displayName,
                  style: const TextStyle(
                    fontSize: 12,
                    fontWeight: FontWeight.w600,
                    color: AppColors.textSecondary,
                  ),
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                ),
              ),
            ],
          ),
        ],
      ],
    );
  }

  Widget _buildAmount(AppLocalizations l10n, bool isIncome) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.end,
      children: [
        Row(
          mainAxisSize: MainAxisSize.min,
          children: [
            Icon(
              isIncome
                  ? Icons.arrow_upward_rounded
                  : Icons.arrow_downward_rounded,
              size: 16,
              color:
                  isIncome ? AppColors.success : AppColors.error,
            ),
            const SizedBox(width: 4),
            Text(
              '\$${transaction.amount.toStringAsFixed(2)}',
              style: TextStyle(
                fontSize: 16,
                fontWeight: FontWeight.w800,
                color: isIncome
                    ? AppColors.success
                    : AppColors.error,
              ),
            ),
          ],
        ),
        const SizedBox(height: 4),
        Text(
          transaction.creator.fullName ?? l10n.unknown,
          style: const TextStyle(
            fontSize: 11,
            fontWeight: FontWeight.w600,
            color: AppColors.textSecondary,
          ),
          maxLines: 1,
          overflow: TextOverflow.ellipsis,
        ),
      ],
    );
  }
}
