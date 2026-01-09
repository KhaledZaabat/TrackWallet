import 'package:famxpense/core/configs/theme/app_colors.dart';
import 'package:flutter/material.dart';

class MyAppBar extends StatelessWidget {
  const MyAppBar({
    super.key,
    required this.title,
    this.actions,
    this.actionsOnPressed,
    this.leading,
    this.leadingOnPressed,
    this.pinned = true,
    this.collapsedBackgroundColor = AppColors.secondary,
    this.enableShadow = true,
  });

  final String title;
  final List<Widget>? actions;
  final Widget? leading;
  final VoidCallback? leadingOnPressed;
  final List<VoidCallback?>? actionsOnPressed;
  final bool pinned;
  final Color collapsedBackgroundColor;
  final bool enableShadow;

  @override
  Widget build(BuildContext context) {
    return SliverLayoutBuilder(
      builder: (context, constraints) {
        const expandedHeight = 150.0;
        final double offset = constraints.scrollOffset;

        // 0 when expanded, 1 when fully collapsed
        final double t =
            (offset / (expandedHeight - kToolbarHeight))
                .clamp(0.0, 1.0);

        // Start and end paddings
        const startExpanded = EdgeInsetsDirectional.only(
          start: 16,
          bottom: 16,
        );
        const startCollapsedWithLeading =
            EdgeInsetsDirectional.only(
          start: 50,
          bottom: 16,
        );

        final hasLeading = leading != null;
        final targetCollapsed = hasLeading
            ? startCollapsedWithLeading
            : startExpanded;

        // Smoothly interpolate padding
        final titlePadding = EdgeInsetsDirectional.lerp(
            startExpanded, targetCollapsed, t)!;

        return SliverAppBar(
          pinned: pinned,
          expandedHeight: expandedHeight,
          backgroundColor: Colors.transparent,
          elevation: 0,
          flexibleSpace: Container(
            decoration: BoxDecoration(
              color: collapsedBackgroundColor,
              boxShadow: enableShadow && t == 1
                  ? [
                      BoxShadow(
                        color: Colors.black
                            .withValues(alpha: 0.2),
                        blurRadius: 10,
                        offset: const Offset(
                            0, 4),
                      ),
                    ]
                  : [],
            ),
            child: FlexibleSpaceBar(
              centerTitle: false,
              titlePadding: titlePadding,
              background: Container(
                  color: AppColors.backgroundColor),
              title: Text(
                title,
                style: TextStyle(
                  color: AppColors.mainBlackShade,
                  fontSize: 25,
                  fontWeight: FontWeight.w900,
                ),
              ),
            ),
          ),
          actions: actions != null
              ? actions!
                  .asMap()
                  .entries
                  .map(
                    (entry) => Padding(
                      padding: const EdgeInsets.only(
                          bottom: 7.0),
                      child: IconButton(
                        icon: entry.value,
                        onPressed: actionsOnPressed !=
                                    null &&
                                actionsOnPressed!.length >
                                    entry.key
                            ? actionsOnPressed![entry.key]
                            : () {
                                ScaffoldMessenger.of(
                                        context)
                                    .showSnackBar(
                                  SnackBar(
                                    content: Text(
                                        'Action ${entry.key + 1} pressed'),
                                  ),
                                );
                              },
                      ),
                    ),
                  )
                  .toList()
              : null,
          leading: leading != null
              ? Padding(
                  padding:
                      const EdgeInsets.only(bottom: 7.0),
                  child: IconButton(
                    icon: leading!,
                    onPressed: leadingOnPressed ??
                        () {
                          ScaffoldMessenger.of(context)
                              .showSnackBar(
                            const SnackBar(
                              content: Text(
                                  'Leading button pressed'),
                            ),
                          );
                        },
                  ),
                )
              : null,
        );
      },
    );
  }
}

