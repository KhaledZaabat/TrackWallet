import 'package:flutter/material.dart';
import 'package:famxpense/features/Invitations/cubit/invitations_cubit.dart';
import 'package:famxpense/l10n/app_localizations.dart';

class SendInvitationDialog extends StatefulWidget {
  final String currentUserEmail;
  final InvitationsCubit cubit;

  const SendInvitationDialog({
    required this.currentUserEmail,
    required this.cubit,
  });

  @override
  State<SendInvitationDialog> createState() => _SendInvitationDialogState();
}

class _SendInvitationDialogState extends State<SendInvitationDialog> {
  final _formKey = GlobalKey<FormState>();
  late TextEditingController _emailController;
  bool _isParent = false;
  bool _isLoading = false;

  @override
  void initState() {
    super.initState();
    _emailController = TextEditingController();
  }

  @override
  void dispose() {
    _emailController.dispose();
    super.dispose();
  }

  String? _validateEmail(String? value) {
    final l10n = AppLocalizations.of(context)!;
    
    if (value == null || value.isEmpty) {
      return l10n.emailRequired;
    }

    final emailRegex = RegExp(
      r'^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$',
    );

    if (!emailRegex.hasMatch(value)) {
      return l10n.invalidEmail;
    }

    if (value.toLowerCase() == widget.currentUserEmail.toLowerCase()) {
      return l10n.cannotInviteYourself;
    }

    return null;
  }

  void _handleSendInvitation() {
    if (_formKey.currentState!.validate()) {
      final email = _emailController.text.trim();

      widget.cubit.sendInvitation(email, _isParent);

      Navigator.of(context).pop();
    }
  }

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context)!;
    
    return AlertDialog(
      title: Text(l10n.sendFamilyInvitation),
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(12),
      ),
      content: SingleChildScrollView(
        child: Form(
          key: _formKey,
          child: Column(
            mainAxisSize: MainAxisSize.min,
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              TextFormField(
                controller: _emailController,
                decoration: InputDecoration(
                  labelText: l10n.emailAddress,
                  hintText: l10n.emailPlaceholder,
                  prefixIcon: const Icon(Icons.email_outlined),
                  border: OutlineInputBorder(
                    borderRadius: BorderRadius.circular(8),
                  ),
                  focusedBorder: OutlineInputBorder(
                    borderRadius: BorderRadius.circular(8),
                    borderSide: const BorderSide(
                      color: Colors.blue,
                      width: 2,
                    ),
                  ),
                  errorBorder: OutlineInputBorder(
                    borderRadius: BorderRadius.circular(8),
                    borderSide: const BorderSide(
                      color: Colors.red,
                      width: 1,
                    ),
                  ),
                ),
                keyboardType: TextInputType.emailAddress,
                validator: _validateEmail,
                textInputAction: TextInputAction.next,
                onChanged: (_) {
                  _formKey.currentState?.validate();
                },
              ),
              const SizedBox(height: 24),

              CheckboxListTile(
                title: Text(l10n.inviteAsParent),
                subtitle: Text(l10n.parentsCanManage),
                value: _isParent,
                onChanged: (newValue) {
                  setState(() {
                    _isParent = newValue ?? false;
                  });
                },
                contentPadding: EdgeInsets.zero,
                controlAffinity: ListTileControlAffinity.leading,
              ),
            ],
          ),
        ),
      ),
      actions: [
        TextButton(
          onPressed: () => Navigator.of(context).pop(),
          child: Text(l10n.cancel),
        ),

        ElevatedButton(
          onPressed: _handleSendInvitation,
          child: Text(l10n.send),
        ),
      ],
    );
  }
}
