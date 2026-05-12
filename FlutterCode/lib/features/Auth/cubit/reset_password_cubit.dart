import 'package:famxpense/data/repos/auth_repository.dart';
import 'package:famxpense/features/Auth/cubit/reset_password_state.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

class ResetPasswordCubit extends Cubit<ResetPasswordState> {
  final AuthRepository _authRepository;
  String? _pendingEmail;

  ResetPasswordCubit(this._authRepository) : super(ResetPasswordInitial());

  Future<void> sendOtp({required String email}) async {
    emit(SendingOtp());

    try {
      final result = await _authRepository.sendResetPasswordOtp(email: email);

      if (result.isSuccess) {
        _pendingEmail = email;
        emit(OtpSent(email: email));
      } else {
        emit(SendOtpError(result.message ?? 'Failed to send OTP'));
        await Future.delayed(const Duration(milliseconds: 100));
        emit(ResetPasswordInitial());
      }
    } catch (e) {
      emit(SendOtpError('An unexpected error occurred'));
      await Future.delayed(const Duration(milliseconds: 100));
      emit(ResetPasswordInitial());
    }
  }

  Future<void> verifyOtp({
    required String otp,
    String? email,
  }) async {
    final emailToVerify = email ?? _pendingEmail;

    if (emailToVerify == null) {
      emit(VerifyOtpError('Email not found. Please try again.'));
      return;
    }

    emit(VerifyingOtp());

    try {
      final result = await _authRepository.verifyResetPasswordOtp(
        email: emailToVerify,
        otp: otp,
      );

      if (result.isSuccess) {
        emit(OtpVerified(email: emailToVerify));
      } else {
        emit(VerifyOtpError(result.message ?? 'Invalid or expired OTP'));
        await Future.delayed(const Duration(milliseconds: 100));
        emit(OtpSent(email: emailToVerify));
      }
    } catch (e) {
      emit(VerifyOtpError('An unexpected error occurred'));
      await Future.delayed(const Duration(milliseconds: 100));
      if (emailToVerify != null) {
        emit(OtpSent(email: emailToVerify));
      }
    }
  }

  Future<void> resendOtp({String? email}) async {
    final emailToResend = email ?? _pendingEmail;

    if (emailToResend == null) {
      emit(ResendOtpError('Email not found. Please try again.'));
      return;
    }

    emit(ResendingOtp());

    try {
      final result = await _authRepository.sendResetPasswordOtp(
        email: emailToResend,
      );

      if (result.isSuccess) {
        emit(OtpResent(result.message ?? 'OTP sent successfully'));
        await Future.delayed(const Duration(seconds: 2));
        emit(OtpSent(email: emailToResend));
      } else {
        emit(ResendOtpError(result.message ?? 'Failed to resend OTP'));
        await Future.delayed(const Duration(milliseconds: 100));
        emit(OtpSent(email: emailToResend));
      }
    } catch (e) {
      emit(ResendOtpError('An unexpected error occurred'));
      await Future.delayed(const Duration(milliseconds: 100));
      if (emailToResend != null) {
        emit(OtpSent(email: emailToResend));
      }
    }
  }

  Future<void> resetPassword({
    required String newPassword,
    String? email,
  }) async {
    final emailToReset = email ?? _pendingEmail;

    if (emailToReset == null) {
      emit(ResetPasswordError('Email not found. Please try again.'));
      return;
    }

    emit(ResettingPassword());

    try {
      final result = await _authRepository.resetPassword(
        email: emailToReset,
        newPassword: newPassword,
      );

      if (result.isSuccess) {
        emit(PasswordResetSuccess());
      } else {
        emit(ResetPasswordError(result.message ?? 'Failed to reset password'));
        await Future.delayed(const Duration(milliseconds: 100));
        emit(OtpVerified(email: emailToReset));
      }
    } catch (e) {
      emit(ResetPasswordError('An unexpected error occurred'));
      await Future.delayed(const Duration(milliseconds: 100));
      if (emailToReset != null) {
        emit(OtpVerified(email: emailToReset));
      }
    }
  }

  void reset() {
    _pendingEmail = null;
    emit(ResetPasswordInitial());
  }

  String? get pendingEmail => _pendingEmail;
}
