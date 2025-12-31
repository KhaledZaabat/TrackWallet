import 'package:famxpense/common/widgets/line_chart.dart';
import 'package:flutter/material.dart';
import 'package:famxpense/common/widgets/models/point_pair.dart';

class AbstractLineChartCard extends StatelessWidget {
  const AbstractLineChartCard({
    super.key,
    required this.points,
    required this.color,
    required this.currency,
  });

  final List<PointPair> points;
  final Color color;
  final String currency;
  @override
  Widget build(BuildContext context) {
    return Container(
      decoration: BoxDecoration(boxShadow: [
        BoxShadow(
          color: Colors.black
              // ignore: deprecated_member_use
              .withValues(alpha: 0.2),
          blurRadius: 10,
          offset: const Offset(0, 0), // shadow offset
        ),
      ]),
      child: LineChartCard(
        points: [points],
        color: color,
        cardBackgroundColor: Colors.white,
        cardBorderRadius: 20,
        showShadow: true,
        endDate: DateTime.now(),
        dateLabelFormatter: (d) => '${d.day} ${[
          'Jan',
          'Feb',
          'Mar',
          'Apr',
          'May',
          'Jun',
          'Jul',
          'Aug',
          'Sep',
          'Oct',
          'Nov',
          'Dec'
        ][d.month - 1]}',
        yLabelFormatter: (v) =>
            '${currency}${v.toStringAsFixed(0)}',
        tooltipFormatter: (date, v) =>
            '${currency}${v.toStringAsFixed(0)}\n${date.day}/${date.month}',
      ),
    );
  }
}

