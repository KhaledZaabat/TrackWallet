import 'package:flutter/material.dart';

class Destination {
  const Destination({
    required this.label,
    required this.icon,
  });

  final String label;
  final IconData icon;
}

const destinations = [
  Destination(label: 'Home', icon: Icons.home),
  Destination(
      label: 'Transactions',
      icon: Icons.attach_money_rounded),
  Destination(label: 'Family', icon: Icons.group),
  Destination(label: 'More', icon: Icons.more_horiz),
];
