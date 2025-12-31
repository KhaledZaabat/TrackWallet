import 'package:famxpense/core/configs/theme/app_colors.dart';
import 'package:famxpense/presentation/more/widgets/theme_mode_pill.dart';
import 'package:famxpense/presentation/more/widgets/actions_tiles.dart';
import 'package:famxpense/presentation/more/widgets/user_info.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:go_router/go_router.dart';
import 'package:google_fonts/google_fonts.dart';

import 'package:famxpense/presentation/more/cubit/user_cubit.dart';
import 'package:famxpense/presentation/more/cubit/user_state.dart';
import 'package:famxpense/core/router/routes.dart'; // ✅

class SettingsPage extends StatelessWidget {
  const SettingsPage({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFFF5F8FA),
      body: SafeArea(
        child: BlocBuilder<UserCubit, UserState>(
          builder: (context, state) {
            // ✅ ensure we actually load the user once
            if (!state.isLoading &&
                state.user == null &&
                state.error == null) {
              context.read<UserCubit>().loadCurrentUser();
              return const Center(
                  child: CircularProgressIndicator());
            }

            if (state.isLoading && state.user == null) {
              return const Center(
                  child: CircularProgressIndicator());
            }

            if (state.error != null && state.user == null) {
              return Center(child: Text(state.error!));
            }

            final user = state.user;

            return SingleChildScrollView(
              child: Padding(
                padding: const EdgeInsets.symmetric(
                    horizontal: 22, vertical: 28),
                child: Column(
                  crossAxisAlignment:
                      CrossAxisAlignment.start,
                  children: [
                    Row(
                      children: [
                        IconButton(
                          icon: Icon(Icons.arrow_back,
                              color: Colors.grey.shade700),
                          onPressed: () => context.pop(),
                        ),
                        const SizedBox(width: 6),
                        Text(
                          'Settings',
                          style: TextStyle(
                            fontSize: 30,
                            fontFamily: GoogleFonts.inter()
                                .fontFamily,
                            fontWeight: FontWeight.w800,
                            color: AppColors.mainBlackShade,
                          ),
                        ),
                      ],
                    ),
                    const SizedBox(height: 22),
                    if (user != null)
                      UserInfo(
                        fullName: user.fullName,
                        userEmail: user.email,
                        profileImageUrl:
                            user.profilePictureUrl.isEmpty
                                ? null
                                : user.profilePictureUrl,
                      ),
                    const SizedBox(height: 26),
                    SizedBox(
                      width: double.infinity,
                      child: ElevatedButton(
                        style: ElevatedButton.styleFrom(
                          backgroundColor:
                              AppColors.primary,
                          padding:
                              const EdgeInsets.symmetric(
                            horizontal: 30,
                            vertical: 12,
                          ),
                          shape: RoundedRectangleBorder(
                            borderRadius:
                                BorderRadius.circular(12),
                          ),
                        ),
                        onPressed: () {
                          // ✅ use router constant
                          context.push(Routes.editProfile);
                        },
                        child: Text(
                          'Edit Profile',
                          style: TextStyle(
                            color: Colors.white,
                            fontSize: 20,
                            fontFamily: GoogleFonts.inter()
                                .fontFamily,
                            fontWeight: FontWeight.w700,
                          ),
                        ),
                      ),
                    ),
                    const SizedBox(height: 26),
                    // ... the rest unchanged ...
                    Text(
                      'Theme',
                      style: TextStyle(
                        color: AppColors.mainBlackShade,
                        fontSize: 16,
                        fontFamily:
                            GoogleFonts.inter().fontFamily,
                        fontWeight: FontWeight.w700,
                      ),
                    ),
                    const SizedBox(height: 10),
                    ActionsTiles(
                      icon: Icons.light_mode,
                      title: 'Theme Mode',
                      additionalAction: ThemeModePill(
                        value: 'Light',
                        onTap: () {},
                      ),
                    ),
                    const SizedBox(height: 26),
                    Text(
                      'Backups',
                      style: TextStyle(
                        color: AppColors.mainBlackShade,
                        fontSize: 16,
                        fontFamily:
                            GoogleFonts.inter().fontFamily,
                        fontWeight: FontWeight.w700,
                      ),
                    ),
                    const SizedBox(height: 10),
                    GestureDetector(
                      onTap: () {},
                      child: const ActionsTiles(
                        icon: Icons.backup,
                        title: 'Export Data',
                      ),
                    ),
                    const SizedBox(height: 18),
                    GestureDetector(
                      onTap: () {},
                      child: const ActionsTiles(
                        icon: Icons.restore,
                        title: 'Import Data',
                      ),
                    ),
                  ],
                ),
              ),
            );
          },
        ),
      ),
    );
  }
}
