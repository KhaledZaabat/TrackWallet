import 'package:famxpense/common/widgets/line_chart.dart';
import 'package:famxpense/common/widgets/models/point_pair.dart';
import 'package:famxpense/core/theme/app_colors.dart';
import 'package:flutter/material.dart';
import 'package:intl/intl.dart';

/// Budget chart widget showing monthly spending trend
class BudgetMonthlyChart extends StatelessWidget {
  final List<PointPair> points;

  const BudgetMonthlyChart({
    super.key,
    required this.points,
  });

  @override
  Widget build(BuildContext context) {
    final currency = NumberFormat.simpleCurrency();
    final now = DateTime.now();

    return LineChartCard(
      points: [points],
      color: AppColors.primary,
      isCurved: true,
      endDate: now,
      cardBackgroundColor: Colors.white,
      cardBorderRadius: 12,
      cardPadding: const EdgeInsets.all(16),
      showShadow: true,
      cardShadowColor: Colors.black.withOpacity(0.05),
      cardElevation: 10,
      dateLabelFormatter: (date) {
        return DateFormat('MMM d').format(date);
      },
      yLabelFormatter: (value) {
        if (value >= 1000) {
          return '${currency.currencySymbol}${(value / 1000).toStringAsFixed(1)}k';
        }
        return '${currency.currencySymbol}${value.toStringAsFixed(0)}';
      },
      tooltipFormatter: (date, value) {
        return '${DateFormat('MMM d, yyyy').format(date)}\n${currency.format(value)}';
      },
      textStyle: const TextStyle(
        fontSize: 11,
        fontWeight: FontWeight.w600,
        color: AppColors.textSecondary,
      ),
    );
  }
}
