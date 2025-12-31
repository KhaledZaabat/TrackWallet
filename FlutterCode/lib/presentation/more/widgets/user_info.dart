import 'package:famxpense/core/configs/theme/app_colors.dart';
import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';

class UserInfo extends StatelessWidget {
  const UserInfo(
      {super.key,
      this.profileImageUrl,
      required this.fullName,
      required this.userEmail});

  final String? profileImageUrl;
  final String fullName;
  final String userEmail;

  @override
  Widget build(BuildContext context) {
    return Row(
      crossAxisAlignment: CrossAxisAlignment.center,
      children: [
        CircleAvatar(
            radius: 35,
            backgroundColor: AppColors.grey,
            backgroundImage: profileImageUrl != null &&
                    profileImageUrl!.startsWith('http')
                ? NetworkImage(profileImageUrl!)
                : null),
        const SizedBox(width: 24),
        Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              fullName,
              style: TextStyle(
                  fontSize: 24,
                  color: AppColors.mainBlackShade,
                  fontFamily:
                      GoogleFonts.inter().fontFamily,
                  fontWeight: FontWeight.w800),
            ),
            SizedBox(height: 4),
            Text(
              userEmail,
              style: TextStyle(
                  fontSize: 14,
                  color: AppColors.mainGrayShade,
                  fontFamily:
                      GoogleFonts.inter().fontFamily,
                  fontWeight: FontWeight.w600),
            ),
          ],
        ),
      ],
    );
  }
}
