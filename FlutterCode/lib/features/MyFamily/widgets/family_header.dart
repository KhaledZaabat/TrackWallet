import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:famxpense/models/Family/family_models.dart';

/// Displays family header information including name, budget, and bio
///
/// This widget shows:
/// - Family name as headline
/// - Current budget formatted as currency
/// - Family bio or placeholder text
///
/// Used at the top of MyFamily page to provide context about the family.
class FamilyHeader extends StatelessWidget {
  final FamilyDetails familyDetails;

  const FamilyHeader({
    required this.familyDetails,
    super.key,
  });

  String _formatCurrency(double amount) {
    final formatter = NumberFormat.currency(symbol: '\$', decimalDigits: 2);
    return formatter.format(amount);
  }

  @override
  Widget build(BuildContext context) {
    return Card(
      margin: const EdgeInsets.all(16),
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(12),
      ),
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // Family name
            Text(
              familyDetails.name,
              style: Theme.of(context).textTheme.headlineSmall?.copyWith(
                    fontWeight: FontWeight.bold,
                  ),
            ),
            const SizedBox(height: 12),

            // Current budget
            Row(
              children: [
                Icon(
                  Icons.account_balance_wallet,
                  color: Colors.green[600],
                  size: 20,
                ),
                const SizedBox(width: 8),
                Text(
                  'Current Budget: ${_formatCurrency(familyDetails.currentBudget)}',
                  style: Theme.of(context).textTheme.bodyLarge?.copyWith(
                        fontWeight: FontWeight.w600,
                      ),
                ),
              ],
            ),
            const SizedBox(height: 12),

            // Family bio
            Text(
              familyDetails.familyBio?.isEmpty ?? true
                  ? 'No family bio yet'
                  : familyDetails.familyBio!,
              style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                    color: Colors.grey[600],
                    fontStyle:
                        (familyDetails.familyBio?.isEmpty ?? true)
                            ? FontStyle.italic
                            : FontStyle.normal,
                  ),
            ),
          ],
        ),
      ),
    );
  }
}
