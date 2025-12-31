import 'package:famxpense/common/widgets/app_bar.dart';
import 'package:famxpense/core/configs/theme/app_colors.dart';
import 'package:famxpense/core/router/routes.dart';
import 'package:famxpense/domain/entities/family.dart';
import 'package:famxpense/presentation/family/cubit/family_cubit.dart';
import 'package:famxpense/presentation/family/cubit/family_state.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:go_router/go_router.dart';
import 'package:intl/intl.dart';

class FamilyPage extends StatelessWidget {
  const FamilyPage({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFFF5F8FA),
      body: BlocBuilder<FamilyCubit, FamilyState>(
        builder: (context, state) {
          if (state.isLoading) {
            return const Center(
                child: CircularProgressIndicator());
          }

          return CustomScrollView(
            slivers: [
              const MyAppBar(title: 'Family'),
              SliverToBoxAdapter(
                child: Padding(
                  padding: const EdgeInsets.symmetric(
                      horizontal: 16, vertical: 14),
                  child: Column(
                    children: [
                      if (state.error != null)
                        Padding(
                          padding:
                              const EdgeInsets.only(bottom: 10),
                          child: Container(
                            width: double.infinity,
                            padding: const EdgeInsets.all(12),
                            decoration: BoxDecoration(
                              color: Colors.red.shade50,
                              borderRadius:
                                  BorderRadius.circular(10),
                              border: Border.all(
                                  color: Colors.red.shade200),
                            ),
                            child: Text(
                              state.error!,
                              style: const TextStyle(
                                color: Colors.redAccent,
                                fontWeight: FontWeight.w600,
                              ),
                            ),
                          ),
                        ),
                      AnimatedSwitcher(
                        duration:
                            const Duration(milliseconds: 300),
                        child: ListView(
                          key: ValueKey(state.selectedFamilyId ??
                              'no-selection'),
                          shrinkWrap: true,
                          physics:
                              const NeverScrollableScrollPhysics(),
                          children: [
                            ...state.families.map(
                              (family) => Padding(
                                padding:
                                    const EdgeInsets.only(
                                        bottom: 12),
                                child: _FamilyCard(
                                  family: family,
                                  isSelected:
                                      family.id ==
                                          state.selectedFamilyId,
                                  onTap: () {
                                    final isSelected =
                                        family.id ==
                                            state.selectedFamilyId;
                                    if (isSelected) {
                                      context.push(
                                        Routes.manageFamily
                                            .replaceFirst(
                                                ':id',
                                                family.id),
                                      );
                                    } else {
                                      context
                                          .read<FamilyCubit>()
                                          .selectFamily(
                                              family.id);
                                    }
                                  },
                                ),
                              ),
                            ),
                            Padding(
                              padding:
                                  const EdgeInsets.only(
                                      bottom: 12),
                              child: _AddFamilyCard(
                                onTap: () {
                                  context.push(
                                      Routes.createFamily);
                                },
                              ),
                            ),
                          ],
                        ),
                      ),
                    ],
                  ),
                ),
              ),
            ],
          );
        },
      ),
    );
  }
}

class _FamilyCard extends StatelessWidget {
  final Family family;
  final bool isSelected;
  final VoidCallback onTap;

  const _FamilyCard({
    required this.family,
    required this.isSelected,
    required this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    final formatter = NumberFormat.compactCurrency(
      symbol: '\$',
      decimalDigits: 0,
    );

    return AnimatedScale(
      scale: isSelected ? 1.02 : 1.0,
      duration: const Duration(milliseconds: 200),
      child: AnimatedContainer(
        duration: const Duration(milliseconds: 250),
        curve: Curves.easeOut,
        decoration: BoxDecoration(
          color: Colors.white,
          borderRadius: BorderRadius.circular(12),
          border: Border.all(
            color: isSelected
                ? AppColors.primary
                : AppColors.stroke,
            width: isSelected ? 1.6 : 1.2,
          ),
          boxShadow: [
            BoxShadow(
              color: Colors.black.withValues(
                  alpha: isSelected ? 0.12 : 0.05),
              blurRadius: isSelected ? 18 : 12,
              offset: const Offset(0, 10),
            ),
          ],
        ),
        child: InkWell(
          borderRadius: BorderRadius.circular(12),
          onTap: onTap,
          child: Padding(
            padding: const EdgeInsets.symmetric(
                horizontal: 16, vertical: 18),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Row(
                  crossAxisAlignment:
                      CrossAxisAlignment.start,
                  children: [
                    Expanded(
                      child: Column(
                        crossAxisAlignment:
                            CrossAxisAlignment.start,
                        children: [
                          Text(
                            family.name,
                            style: const TextStyle(
                              fontSize: 18,
                              fontWeight: FontWeight.w800,
                              color: AppColors.mainBlackShade,
                            ),
                          ),
                          const SizedBox(height: 6),
                          Text(
                            formatter
                                .format(family.currentBudget),
                            style: TextStyle(
                              fontSize: 15,
                              fontWeight: FontWeight.w700,
                              color: isSelected
                                  ? const Color(0xFF27AE60)
                                  : AppColors.mainBlackShade,
                            ),
                          ),
                        ],
                      ),
                    ),
                    Container(
                      width: 16,
                      height: 16,
                      decoration: BoxDecoration(
                        shape: BoxShape.circle,
                        color: isSelected
                            ? AppColors.primary
                            : Colors.grey.shade500,
                      ),
                    ),
                  ],
                ),
                const SizedBox(height: 18),
                Text(
                  'Members • manage',
                  style: TextStyle(
                    fontSize: 12,
                    fontWeight: FontWeight.w600,
                    color: Colors.grey.shade500,
                  ),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}

class _AddFamilyCard extends StatelessWidget {
  final VoidCallback onTap;
  const _AddFamilyCard({required this.onTap});

  @override
  Widget build(BuildContext context) {
    return InkWell(
      borderRadius: BorderRadius.circular(12),
      onTap: onTap,
      child: Container(
        height: 120,
        decoration: BoxDecoration(
          color: Colors.white,
          borderRadius: BorderRadius.circular(12),
          border: Border.all(
            color: AppColors.stroke,
            width: 1.2,
          ),
        ),
        child: Center(
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: const [
              Icon(
                Icons.add,
                size: 24,
                color: AppColors.mainGrayShade,
              ),
              SizedBox(height: 6),
              Text(
                'Family',
                style: TextStyle(
                  color: AppColors.mainGrayShade,
                  fontWeight: FontWeight.w700,
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
