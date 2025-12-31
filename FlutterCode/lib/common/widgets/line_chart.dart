import 'package:fl_chart/fl_chart.dart';
import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:famxpense/common/widgets/models/point_pair.dart';

typedef AxisLabelFormatter = String Function(double value);
typedef DateLabelFormatter = String Function(DateTime date);
typedef TooltipFormatter = String Function(
    DateTime date, double value);

class LineChartCard extends StatelessWidget {
  const LineChartCard({
    super.key,
    required this.points,
    this.isCurved = false,
    this.color,
    this.endDate,
    this.verticalLineAt,
    this.horizontalLineAt,
    this.enableTouch = true,
    this.seriesColors = const [],
    this.keepHorizontalLineInView = false,
    this.extraLeftPaddingIfSmall = 0,
    this.amountBefore = 0,
    this.removeZeroEntries = false,
    this.showCumulativeSpending = false,
    this.isFullScreen = false,
    this.dateLabelFormatter,
    this.yLabelFormatter,
    this.tooltipFormatter,
    this.textStyle = const TextStyle(fontSize: 13),
    this.gridColor,
    this.zeroLineColor,
    // card styling
    this.cardBackgroundColor = const Color(0xFFF6F7FA),
    this.cardBorderRadius = 24,
    this.cardPadding = const EdgeInsets.all(16),
    this.cardShadowColor = const Color(0x1A000000),
    this.cardElevation = 8,
    this.showShadow = true,
  });

  final List<List<PointPair>> points;
  final bool isCurved;
  final Color? color;
  final DateTime? endDate;
  final double? verticalLineAt;
  final double? horizontalLineAt;
  final bool enableTouch;
  final List<Color> seriesColors;
  final bool keepHorizontalLineInView;
  final double extraLeftPaddingIfSmall;
  final double amountBefore;

  final bool removeZeroEntries;
  final bool showCumulativeSpending;
  final bool isFullScreen;

  final DateLabelFormatter? dateLabelFormatter;
  final AxisLabelFormatter? yLabelFormatter;
  final TooltipFormatter? tooltipFormatter;

  final TextStyle textStyle;
  final Color? gridColor;
  final Color? zeroLineColor;

  final Color cardBackgroundColor;
  final double cardBorderRadius;
  final EdgeInsets cardPadding;
  final Color cardShadowColor;
  final double cardElevation;
  final bool showShadow;

  // ------------ helpers ------------

  List<List<PointPair>> _filterPointsList(
      List<List<PointPair>> pointsList) {
    return pointsList.map(_filterPoints).toList();
  }

  List<PointPair> _filterPoints(List<PointPair> pts) {
    if (!removeZeroEntries) return pts;

    final out = <PointPair>[];

    if (!showCumulativeSpending) {
      for (final p in pts) {
        if (p.y != 0) out.add(PointPair(p.x, p.y));
      }
      if (out.isEmpty) return [PointPair(0, 0)];
      if (out.last.x != pts.last.x) {
        out.add(PointPair(pts.last.x, 0));
      }
      return out;
    } else {
      if (pts.isEmpty) return [PointPair(0, 0)];
      out.add(PointPair(pts.first.x, pts.first.y));
      double prev = 0;
      for (final p in pts) {
        if (p.y != prev) out.add(PointPair(p.x, p.y));
        prev = p.y;
      }
      if (out.isEmpty) return [PointPair(0, 0)];
      if (out.last.x != pts.last.x) {
        out.add(PointPair(pts.last.x, pts.last.y));
      }
      return out;
    }
  }

  List<List<FlSpot>> _convertPoints(
      List<List<PointPair>> pointsList) {
    return pointsList
        .map((list) =>
            list.map((p) => FlSpot(p.x, p.y)).toList())
        .toList();
  }

  PointPair _getMaxPoint(List<List<PointPair>> pointsList) {
    final max = PointPair(0, 0);
    if (amountBefore != 0 &&
        pointsList.isNotEmpty &&
        pointsList[0].isNotEmpty) {
      max.y = pointsList[0][0].y;
    }
    for (final list in pointsList) {
      for (final p in list) {
        if (p.x > max.x) max.x = p.x;
        if (p.y > max.y) max.y = p.y;
      }
    }
    if (keepHorizontalLineInView &&
        horizontalLineAt != null &&
        horizontalLineAt != double.infinity &&
        max.y <
            (horizontalLineAt! + horizontalLineAt! * 0.1)) {
      max.y = horizontalLineAt! + horizontalLineAt! * 0.1;
    }
    return max;
  }

  PointPair _getMinPoint(List<List<PointPair>> pointsList) {
    final min = PointPair(0, 0);
    if (amountBefore != 0 &&
        pointsList.isNotEmpty &&
        pointsList[0].isNotEmpty) {
      min.y = pointsList[0][0].y;
    }
    for (final list in pointsList) {
      for (final p in list) {
        if (p.x < min.x) min.x = p.x;
        if (p.y < min.y) min.y = p.y;
      }
    }
    return min;
  }

  @override
  Widget build(BuildContext context) {
    final filtered = _filterPointsList(points);
    final maxPair = _getMaxPoint(filtered);
    final minPair = _getMinPoint(filtered);

    if (maxPair.y == minPair.y) {
      maxPair.y = maxPair.y + 1;
    }

    final baseColor =
        color ?? Theme.of(context).colorScheme.primary;
    final chartHeight =
        MediaQuery.sizeOf(context).width > 700
            ? 300.0
            : 175.0;

    return ClipRRect(
      borderRadius: BorderRadius.circular(cardBorderRadius),
      child: Container(
        decoration: BoxDecoration(
          color: cardBackgroundColor,
          borderRadius:
              BorderRadius.circular(cardBorderRadius),
          boxShadow: showShadow
              ? [
                  BoxShadow(
                    color: cardShadowColor,
                    blurRadius: cardElevation,
                    offset: const Offset(0, 6),
                  ),
                ]
              : [],
        ),
        child: Padding(
          padding: cardPadding,
          child: SizedBox(
            height: chartHeight,
            child: _LineChart(
              spots: _convertPoints(filtered),
              maxPair: maxPair,
              minPair: minPair,
              color: baseColor,
              isCurved: isCurved,
              endDate: endDate,
              verticalLineAt: verticalLineAt,
              horizontalLineAt: horizontalLineAt,
              enableTouch: enableTouch,
              colors: seriesColors,
              extraLeftPaddingIfSmall:
                  extraLeftPaddingIfSmall,
              amountBefore: amountBefore,
              isFullScreen: isFullScreen,
              dateLabelFormatter: dateLabelFormatter,
              yLabelFormatter: yLabelFormatter,
              tooltipFormatter: tooltipFormatter,
              textStyle: textStyle,
              gridColor:
                  gridColor ?? baseColor.withValues(alpha: 0.15),
              zeroLineColor: zeroLineColor ??
                  baseColor.withValues(alpha: 0.4),
            ),
          ),
        ),
      ),
    );
  }
}

// ---------------- internal chart ----------------

class _LineChart extends StatefulWidget {
  const _LineChart({
    required this.spots,
    required this.maxPair,
    required this.minPair,
    required this.color,
    required this.isCurved,
    required this.endDate,
    required this.verticalLineAt,
    required this.horizontalLineAt,
    required this.enableTouch,
    required this.extraLeftPaddingIfSmall,
    required this.amountBefore,
    required this.isFullScreen,
    required this.colors,
    required this.dateLabelFormatter,
    required this.yLabelFormatter,
    required this.tooltipFormatter,
    required this.textStyle,
    required this.gridColor,
    required this.zeroLineColor,
  });

  final List<List<FlSpot>> spots;
  final PointPair maxPair;
  final PointPair minPair;
  final Color color;
  final List<Color> colors;
  final bool isCurved;
  final DateTime? endDate;
  final double? verticalLineAt;
  final double? horizontalLineAt;
  final bool enableTouch;
  final double extraLeftPaddingIfSmall;
  final double amountBefore;
  final bool isFullScreen;

  final DateLabelFormatter? dateLabelFormatter;
  final AxisLabelFormatter? yLabelFormatter;
  final TooltipFormatter? tooltipFormatter;
  final TextStyle textStyle;
  final Color gridColor;
  final Color zeroLineColor;

  @override
  State<_LineChart> createState() => _LineChartState();
}

class _LineChartState extends State<_LineChart> {
  bool loaded = false;
  double extraHorizontalPadding = 10;

  FlGridData get gridData => FlGridData(
        show: true,
        drawVerticalLine: true,
        drawHorizontalLine: true,
        verticalInterval: (() {
          final span =
              (widget.maxPair.x - widget.minPair.x).abs();
          if (span == 0) return 1.0;
          return span / (widget.isFullScreen ? 6 : 4);
        })(),
        horizontalInterval: (() {
          final span =
              (widget.maxPair.y - widget.minPair.y).abs();
          if (double.parse(span.toStringAsFixed(5)) == 0.0)
            return 0.001;
          return span / (widget.isFullScreen ? 7 : 4);
        })(),
        getDrawingVerticalLine: (value) => FlLine(
          color: widget.gridColor.withValues(alpha: 0.3),
          strokeWidth: 1,
          dashArray: const [2, 8],
        ),
        getDrawingHorizontalLine: (value) => FlLine(
          color: widget.gridColor.withValues(alpha: 0.3),
          strokeWidth: 1,
          dashArray: const [2, 8],
        ),
      );

  @override
  void initState() {
    super.initState();
    Future.microtask(() {
      if (mounted) {
        setState(() => loaded = true);
      }
    });
  }

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: EdgeInsets.only(
          right: 15 + extraHorizontalPadding,
          top: 8,
          bottom: 0),
      child: LineChart(
        data,
        duration: const Duration(milliseconds: 800),
        curve: Curves.fastLinearToSlowEaseIn,
      ),
    );
  }

  LineChartData get data => LineChartData(
        lineTouchData: lineTouchData,
        gridData: gridData,
        borderData: FlBorderData(show: false),
        lineBarsData: lineBarsData,
        minX: 0,
        maxX: loaded
            ? widget.maxPair.x
            : widget.maxPair.x * 0.3,
        minY: loaded
            ? (widget.minPair.y == 0
                ? -0.000001
                : widget.minPair.y)
            : widget.minPair.y -
                (widget.minPair.y - widget.amountBefore) *
                    0.7,
        maxY: loaded
            ? (widget.maxPair.y == 0
                ? 0.000001
                : widget.maxPair.y)
            : widget.maxPair.y +
                (widget.maxPair.y - widget.amountBefore) *
                    0.7,
        titlesData: titlesData,
        extraLinesData: extraLinesData,
      );

  ExtraLinesData get extraLinesData => ExtraLinesData(
        horizontalLines: [
          HorizontalLine(
            y: 0,
            color: widget.zeroLineColor,
            strokeWidth: 2,
          ),
          if (widget.horizontalLineAt != null)
            HorizontalLine(
              y: widget.horizontalLineAt!,
              color: widget.zeroLineColor.withValues(alpha: 0.9),
              dashArray: const [2, 2],
              strokeWidth: 2,
            ),
        ],
        verticalLines: [
          VerticalLine(
            x: 0.0001,
            dashArray: const [2, 5],
            strokeWidth: 2,
            color: widget.gridColor.withValues(alpha: 0.8),
          ),
          if (widget.verticalLineAt != null)
            VerticalLine(
              x: widget.maxPair.x - widget.verticalLineAt!,
              dashArray: const [2, 2],
              strokeWidth: 2,
              color: widget.gridColor.withValues(alpha: 0.9),
            ),
        ],
      );

  FlTitlesData get titlesData => FlTitlesData(
        show: true,
        bottomTitles: AxisTitles(
          sideTitles: SideTitles(
            showTitles: true,
            reservedSize: 32,
            interval: widget.maxPair.x /
                        (widget.isFullScreen ? 6 : 4) ==
                    0
                ? 5
                : widget.maxPair.x /
                    (widget.isFullScreen ? 6 : 4),
            getTitlesWidget: (value, _) {
              final currentDate =
                  widget.endDate ?? DateTime.now();
              final dayOffset =
                  -widget.maxPair.x.toInt() + value.round();
              final date = currentDate
                  .add(Duration(days: dayOffset));
              final text = widget.dateLabelFormatter != null
                  ? widget.dateLabelFormatter!(date)
                  : '${date.day}/${date.month}';

              return Padding(
                padding: const EdgeInsets.only(top: 8.0),
                child: Text(
                  text,
                  maxLines: 1,
                  textAlign: TextAlign.center,
                  style: widget.textStyle.copyWith(
                    color:
                        widget.gridColor.withValues(alpha: 0.7),
                  ),
                ),
              );
            },
          ),
        ),
        leftTitles: AxisTitles(
          sideTitles: SideTitles(
            showTitles: true,
            reservedSize: 40 +
                widget.extraLeftPaddingIfSmall +
                extraHorizontalPadding,
            interval: (() {
              final diff =
                  (widget.maxPair.y - widget.minPair.y)
                      .abs();
              if (double.parse(diff.toStringAsFixed(5)) ==
                  0.0) return 0.001;
              return diff / (widget.isFullScreen ? 7 : 4);
            })(),
            getTitlesWidget: (value, meta) {
              if (value == meta.max || value == meta.min) {
                return const SizedBox.shrink();
              }

              final label = widget.yLabelFormatter != null
                  ? widget.yLabelFormatter!(value)
                  : value.toStringAsFixed(1);

              return Padding(
                padding: const EdgeInsets.only(right: 8.0),
                child: Text(
                  label,
                  maxLines: 1,
                  overflow: TextOverflow.fade,
                  softWrap: false,
                  textAlign: TextAlign.end,
                  style: widget.textStyle.copyWith(
                    color:
                        widget.gridColor.withValues(alpha: 0.7),
                  ),
                ),
              );
            },
          ),
        ),
        topTitles: const AxisTitles(
            sideTitles: SideTitles(showTitles: false)),
        rightTitles: const AxisTitles(
            sideTitles: SideTitles(showTitles: false)),
      );

  int? touchedValue;

  LineTouchData get lineTouchData => LineTouchData(
        enabled: widget.enableTouch,
        touchSpotThreshold: 1000,
        getTouchedSpotIndicator: (LineChartBarData barData,
            List<int> spotIndexes) {
          final lineColor = barData.color ?? widget.color;
          return spotIndexes
              .map(
                (index) => TouchedSpotIndicatorData(
                  FlLine(
                    color: lineColor.withValues(alpha: 0.9),
                    strokeWidth: 2,
                    dashArray: const [2, 2],
                  ),
                  FlDotData(
                    show: true,
                    getDotPainter:
                        (spot, percent, barData, index) =>
                            FlDotCirclePainter(
                      radius: 3,
                      color: lineColor.withValues(alpha: 0.9),
                      strokeWidth: 2,
                      strokeColor:
                          lineColor.withValues(alpha: 0.9),
                    ),
                  ),
                ),
              )
              .toList();
        },
        touchCallback: (event, response) {
          if (!event.isInterestedForInteractions ||
              response == null) {
            touchedValue = null;
            return;
          }
          final value = response.lineBarSpots![0].x;
          if (event.runtimeType == FlLongPressStart) {
            HapticFeedback.selectionClick();
          } else if (touchedValue != value.toInt() &&
              (event.runtimeType == FlLongPressMoveUpdate ||
                  event.runtimeType == FlPanUpdateEvent)) {
            HapticFeedback.selectionClick();
          }
          touchedValue = value.toInt();
        },
        touchTooltipData: LineTouchTooltipData(
          getTooltipColor: (_) =>
              widget.color.withValues(alpha: 0.7),
          tooltipRoundedRadius: 8,
          fitInsideVertically: true,
          fitInsideHorizontally: true,
          tooltipPadding: const EdgeInsets.symmetric(
              horizontal: 8, vertical: 4),
          getTooltipItems: (spots) {
            final currentDate =
                widget.endDate ?? DateTime.now();
            return spots.map((s) {
              final date = currentDate.add(
                Duration(
                  days: -widget.maxPair.x.toInt() +
                      s.x.toInt(),
                ),
              );
              final text = widget.tooltipFormatter != null
                  ? widget.tooltipFormatter!(date, s.y)
                  : '${date.day}/${date.month}\n${s.y.toStringAsFixed(2)}';
              return LineTooltipItem(
                text,
                const TextStyle(
                  color: Colors.white,
                  fontWeight: FontWeight.bold,
                  fontSize: 12,
                ),
              );
            }).toList();
          },
        ),
      );

  List<LineChartBarData> get lineBarsData => [
        for (int i = 0; i < widget.spots.length; i++)
          _lineChartBarData(widget.spots[i], i),
      ];

  LineChartBarData _lineChartBarData(
      List<FlSpot> spots, int index) {
    final base = widget.colors.isNotEmpty
        ? widget.colors[index % widget.colors.length]
        : widget.color;

    return LineChartBarData(
      color: base.withValues(alpha: 0.9),
      barWidth: 3,
      isStrokeCapRound: true,
      dotData: FlDotData(show: false),
      isCurved: widget.isCurved,
      curveSmoothness: 0.3,
      preventCurveOverShooting: true,
      preventCurveOvershootingThreshold: 8,
      aboveBarData: BarAreaData(
        applyCutOffY: true,
        cutOffY: 0,
        show: true,
        gradient: LinearGradient(
          colors: [
            base.withValues(alpha: 0.3),
            Colors.transparent
          ],
          begin: Alignment.topCenter,
          end: Alignment.bottomCenter,
        ),
      ),
      belowBarData: BarAreaData(
        applyCutOffY: true,
        cutOffY: 0,
        show: true,
        gradient: LinearGradient(
          colors: [
            base.withValues(alpha: 0.3),
            Colors.transparent
          ],
          begin: Alignment.bottomCenter,
          end: Alignment.topCenter,
        ),
      ),
      spots: spots,
    );
  }
}

