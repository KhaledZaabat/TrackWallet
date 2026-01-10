// ignore: unused_import
import 'package:intl/intl.dart' as intl;
import 'app_localizations.dart';

// ignore_for_file: type=lint

/// The translations for French (`fr`).
class AppLocalizationsFr extends AppLocalizations {
  AppLocalizationsFr([String locale = 'fr']) : super(locale);

  @override
  String get settings => 'Paramètres';

  @override
  String get security => 'Sécurité';

  @override
  String get changePassword => 'Changer le mot de passe';

  @override
  String get changePasswordSubtitle =>
      'Mettre à jour le mot de passe de votre compte';

  @override
  String get notifications => 'Notifications';

  @override
  String get notificationRequirement =>
      'Au moins une méthode de notification doit être activée';

  @override
  String get emailNotifications => 'Notifications par e-mail';

  @override
  String get emailNotificationsSubtitle =>
      'Recevoir des notifications par e-mail';

  @override
  String get pushNotifications => 'Notifications push';

  @override
  String get pushNotificationsSubtitle => 'Recevoir des notifications push';

  @override
  String get account => 'Compte';

  @override
  String get logout => 'Déconnexion';

  @override
  String get logoutSubtitle => 'Se déconnecter de votre compte';

  @override
  String get logoutConfirmTitle => 'Déconnexion';

  @override
  String get logoutConfirmMessage =>
      'Êtes-vous sûr de vouloir vous déconnecter ? Vous devrez vous reconnecter pour accéder à votre compte.';

  @override
  String get cancel => 'Annuler';

  @override
  String get viewProfile => 'Voir le profil';

  @override
  String get retry => 'Réessayer';

  @override
  String get noUserData => 'Aucune donnée utilisateur disponible';

  @override
  String get currentPassword => 'Mot de passe actuel';

  @override
  String get newPassword => 'Nouveau mot de passe';

  @override
  String get confirmPassword => 'Confirmer le mot de passe';

  @override
  String get pleaseEnterCurrentPassword =>
      'Veuillez entrer votre mot de passe actuel';

  @override
  String get pleaseEnterNewPassword =>
      'Veuillez entrer un nouveau mot de passe';

  @override
  String get passwordMinLength =>
      'Le mot de passe doit comporter au moins 6 caractères';

  @override
  String get pleaseConfirmPassword => 'Veuillez confirmer votre mot de passe';

  @override
  String get passwordsDoNotMatch => 'Les mots de passe ne correspondent pas';

  @override
  String get update => 'Mettre à jour';

  @override
  String get cannotDisableAllNotifications =>
      'Impossible de désactiver toutes les notifications';

  @override
  String get cannotDisableAllNotificationsMessage =>
      'Au moins une méthode de notification doit rester activée.';

  @override
  String get gotIt => 'Compris';

  @override
  String get language => 'Langue';

  @override
  String get languageSubtitle => 'Choisissez votre langue préférée';

  @override
  String get selectLanguage => 'Sélectionner la langue';

  @override
  String get english => 'Anglais';

  @override
  String get french => 'Français';

  @override
  String get myFamily => 'Ma Famille';

  @override
  String get leaveFamily => 'Quitter la famille';

  @override
  String get leaveFamilyConfirmMessage =>
      'Êtes-vous sûr de vouloir quitter cette famille ?\n\nVos transactions resteront avec la famille.';

  @override
  String get leave => 'Quitter';

  @override
  String get failedToLoadFamily => 'Échec du chargement de la famille';

  @override
  String get currentBudget => 'Budget actuel';

  @override
  String memberCount(num count) {
    String _temp0 = intl.Intl.pluralLogic(
      count,
      locale: localeName,
      other: '$count membres',
      one: '1 membre',
    );
    return '$_temp0';
  }

  @override
  String get addFamilyBio => 'Ajouter une bio familiale';

  @override
  String get editFamily => 'Modifier la famille';

  @override
  String get familyName => 'Nom de la famille';

  @override
  String get enterFamilyName => 'Entrez le nom de la famille';

  @override
  String get familyNameRequired => 'Le nom de la famille est requis';

  @override
  String get familyNameMinLength =>
      'Le nom doit comporter au moins 2 caractères';

  @override
  String get familyBioOptional => 'Bio familiale (Optionnel)';

  @override
  String get describeFamilyHint => 'Décrivez votre famille...';

  @override
  String get saveChanges => 'Enregistrer les modifications';

  @override
  String get members => 'Membres';

  @override
  String get noFamilyMembersYet => 'Aucun membre de la famille encore';

  @override
  String get inviteMembersToSeeHere => 'Invitez des membres pour les voir ici';

  @override
  String get parent => 'Parent';

  @override
  String get you => 'Vous';

  @override
  String get removeMember => 'Retirer le membre';

  @override
  String removeMemberConfirmMessage(Object memberName) {
    return 'Êtes-vous sûr de vouloir retirer $memberName de la famille ?';
  }

  @override
  String get remove => 'Retirer';

  @override
  String get profile => 'Profil';

  @override
  String get personalInformation => 'Informations personnelles';

  @override
  String get fullName => 'Nom complet';

  @override
  String get fullNameRequired => 'Le nom complet est requis';

  @override
  String get fullNameMinLength =>
      'Le nom complet doit comporter au moins 3 caractères';

  @override
  String get gender => 'Genre';

  @override
  String get male => 'Homme';

  @override
  String get female => 'Femme';

  @override
  String get pleaseSelectGender => 'Veuillez sélectionner le genre';

  @override
  String get dateOfBirth => 'Date de naissance';

  @override
  String get pleaseSelectBirthDate =>
      'Veuillez sélectionner votre date de naissance';

  @override
  String get updateProfile => 'Mettre à jour le profil';

  @override
  String get cannotUpdateProfileNow => 'Impossible de mettre à jour le profil';

  @override
  String get chooseFromGallery => 'Choisir dans la galerie';

  @override
  String get takePhoto => 'Prendre une photo';

  @override
  String get transactions => 'Transactions';

  @override
  String get transaction => 'Transaction';

  @override
  String get unknown => 'Inconnu';

  @override
  String filtersActive(num count) {
    String _temp0 = intl.Intl.pluralLogic(
      count,
      locale: localeName,
      other: '$count filtres actifs',
      one: '1 filtre actif',
    );
    return '$_temp0';
  }

  @override
  String get clear => 'Effacer';

  @override
  String get somethingWentWrong => 'Oups ! Une erreur s\'est produite';

  @override
  String get tryAgain => 'Réessayer';

  @override
  String get noMatchingTransactions => 'Aucune transaction correspondante';

  @override
  String get noTransactionsYet => 'Pas encore de transactions';

  @override
  String get adjustFiltersHint => 'Essayez d\'ajuster vos filtres';

  @override
  String get startTrackingHint => 'Commencez à suivre vos dépenses';

  @override
  String get clearFilters => 'Effacer les filtres';

  @override
  String get today => 'Aujourd\'hui';

  @override
  String get yesterday => 'Hier';

  @override
  String get transactionCreated => 'Transaction créée avec succès';

  @override
  String get transactionUpdated => 'Transaction mise à jour avec succès';

  @override
  String get transactionDeleted => 'Transaction supprimée avec succès';

  @override
  String get deleteTransaction => 'Supprimer la transaction ?';

  @override
  String get deleteTransactionMessage => 'Cette action est irréversible.';

  @override
  String get delete => 'Supprimer';

  @override
  String get editTransaction => 'Modifier la transaction';

  @override
  String get newTransaction => 'Nouvelle transaction';

  @override
  String get expense => 'Dépense';

  @override
  String get income => 'Revenu';

  @override
  String get amount => 'Montant';

  @override
  String get enterAmount => 'Entrez le montant';

  @override
  String get invalidAmount => 'Montant invalide';

  @override
  String get titleOptional => 'Titre (Optionnel)';

  @override
  String get whatIsThisFor => 'C\'est pour quoi ?';

  @override
  String get category => 'Catégorie';

  @override
  String get selectCategory => 'Sélectionner une catégorie';

  @override
  String get searchCategory => 'Rechercher une catégorie';

  @override
  String get noCategoriesFound => 'Aucune catégorie trouvée';

  @override
  String get tryDifferentKeywords => 'Essayez avec d\'autres mots-clés';

  @override
  String get date => 'Date';

  @override
  String get notesOptional => 'Notes (Optionnel)';

  @override
  String get addAdditionalDetails => 'Ajoutez des détails supplémentaires...';

  @override
  String get updateTransaction => 'Mettre à jour la transaction';

  @override
  String get saveTransaction => 'Enregistrer la transaction';

  @override
  String get pleaseSelectCategory => 'Veuillez sélectionner une catégorie';

  @override
  String get amountGreaterThanZero => 'Le montant doit être supérieur à zéro';

  @override
  String get filterTransactions => 'Filtrer les transactions';

  @override
  String get transactionType => 'Type de transaction';

  @override
  String get all => 'Tous';

  @override
  String get categoryGroup => 'Groupe de catégories';

  @override
  String get allCategories => 'Toutes les catégories';

  @override
  String get amountRange => 'Plage de montant';

  @override
  String get min => 'Min';

  @override
  String get max => 'Max';

  @override
  String get createdBy => 'Créé par';

  @override
  String get noFamilyMembersFound => 'Aucun membre trouvé';

  @override
  String get allMembers => 'Tous les membres';

  @override
  String get clearAll => 'Tout effacer';

  @override
  String applyFiltersCount(Object count) {
    return 'Appliquer les filtres ($count)';
  }

  @override
  String get showAll => 'Afficher tout';

  @override
  String get familyInvitations => 'Invitations familiales';

  @override
  String get received => 'Reçues';

  @override
  String get sent => 'Envoyées';

  @override
  String get sendInvitation => 'Envoyer une invitation';

  @override
  String get noPendingInvitations => 'Aucune invitation en attente';

  @override
  String get checkBackLater => 'Revenez plus tard';

  @override
  String get failedToLoadInvitations => 'Échec du chargement des invitations';

  @override
  String get invitations => 'Invitations';

  @override
  String get families => 'Familles';

  @override
  String toLabel(Object email) {
    return 'À : $email';
  }

  @override
  String fromLabel(Object name) {
    return 'De : $name';
  }

  @override
  String joinFamily(Object familyName) {
    return 'Rejoindre $familyName';
  }

  @override
  String get roleParent => 'Rôle : Parent';

  @override
  String get roleMember => 'Rôle : Membre';

  @override
  String get cancelInvite => 'Annuler l\'invitation';

  @override
  String get decline => 'Refuser';

  @override
  String get accept => 'Accepter';

  @override
  String get sendFamilyInvitation => 'Envoyer une invitation familiale';

  @override
  String get emailAddress => 'Adresse e-mail';

  @override
  String get emailPlaceholder => 'utilisateur@exemple.com';

  @override
  String get emailRequired => 'L\'e-mail est requis';

  @override
  String get invalidEmail => 'Veuillez entrer une adresse e-mail valide';

  @override
  String get cannotInviteYourself => 'Vous ne pouvez pas vous inviter';

  @override
  String get inviteAsParent => 'Inviter en tant que parent';

  @override
  String get parentsCanManage => 'Les parents peuvent gérer les invitations';

  @override
  String get send => 'Envoyer';

  @override
  String get createFamily => 'Créer une famille';

  @override
  String get startFamilyJourney => 'Démarrez votre aventure familiale';

  @override
  String get createFamilyDescription =>
      'Créez une famille pour gérer les dépenses ensemble';

  @override
  String get familyNameRequired2 => 'Veuillez entrer un nom de famille';

  @override
  String get familyNameHint => 'La Famille Dupont';

  @override
  String get initialBudget => 'Budget initial';

  @override
  String get enterInitialBudget => 'Veuillez entrer un budget initial';

  @override
  String get enterValidNumber => 'Veuillez entrer un nombre valide';

  @override
  String get budgetCannotBeNegative => 'Le budget ne peut pas être négatif';

  @override
  String get shortFamilyDescription =>
      'Une courte description de votre famille...';

  @override
  String familyCreatedSuccess(Object familyName) {
    return 'Famille \"$familyName\" créée !';
  }

  @override
  String get createFamilyInfo =>
      'Vous deviendrez automatiquement l\'administrateur de cette famille. Vous pourrez inviter d\'autres membres depuis les paramètres de la famille.';

  @override
  String get selectAFamily => 'Sélectionner une famille';

  @override
  String get createYourFirstFamily => 'Créez votre première famille';

  @override
  String get chooseFamily => 'Choisissez une famille pour continuer';

  @override
  String get noFamiliesYet => 'Pas encore de familles';

  @override
  String get createFirstFamilyHint =>
      'Créez votre première famille pour commencer à gérer les dépenses ensemble.';

  @override
  String get createYourFirstFamilyButton => 'Créer votre première famille';

  @override
  String get failedToLoadFamilies => 'Échec du chargement des familles';

  @override
  String get deleteFamily => 'Supprimer la famille';

  @override
  String deleteFamilyConfirmMessage(Object familyName) {
    return 'Êtes-vous sûr de vouloir supprimer définitivement \"$familyName\" ?\n\nCela supprimera tous les membres et annulera les invitations en attente. Les transactions seront conservées.';
  }

  @override
  String get member => 'Membre';

  @override
  String get welcomeBack => 'Bienvenue,';

  @override
  String get budgetThisMonth => 'Budget ce mois-ci';

  @override
  String get recentTransactions => 'Transactions récentes';

  @override
  String get viewAll => 'Voir tout';

  @override
  String get noTransactionsYetDashboard => 'Pas encore de transactions';

  @override
  String get dashboard => 'Tableau de bord';

  @override
  String get goodToSeeYou => 'Content de vous revoir !';

  @override
  String get letsContinueJourney => 'Continuons l\'aventure.';

  @override
  String get or => 'OU';

  @override
  String get dontHaveAccount => 'Vous n\'avez pas de compte ?';

  @override
  String get signUp => 'S\'inscrire';

  @override
  String get needToVerifyAccount => 'Besoin de vérifier votre compte ?';

  @override
  String get verify => 'Vérifier';

  @override
  String get verifyAccount => 'Vérifier le compte';

  @override
  String get resendVerificationCode =>
      'Entrez votre e-mail pour renvoyer le code de vérification';

  @override
  String get continueButton => 'Continuer';

  @override
  String get resetPassword => 'Réinitialiser le mot de passe';

  @override
  String get resetPasswordInstructions =>
      'Entrez votre e-mail pour recevoir les instructions de réinitialisation';

  @override
  String get sendResetLink => 'Envoyer le lien';

  @override
  String get createAccount => 'Créer un compte';

  @override
  String get fillDetailsToStart => 'Remplissez vos informations pour commencer';

  @override
  String get username => 'Nom d\'utilisateur';

  @override
  String get usernameHint => 'ExpenseTracker1';

  @override
  String get email => 'E-mail';

  @override
  String get password => 'Mot de passe';

  @override
  String get selectDateHint => 'Sélectionner une date';

  @override
  String get selectGender => 'Sélectionner';

  @override
  String get alreadyHaveAccount => 'Vous avez déjà un compte ?';

  @override
  String get logIn => 'Se connecter';

  @override
  String get removePhoto => 'Supprimer la photo';

  @override
  String get failedToPickImage => 'Échec de la sélection de l\'image';

  @override
  String get failedToTakePhoto => 'Échec de la prise de photo';

  @override
  String get forgotPassword => 'Mot de passe oublié ?';

  @override
  String get forgotPasswordInstructions =>
      'Ne vous inquiétez pas ! Entrez votre e-mail et nous vous enverrons un code de vérification pour réinitialiser votre mot de passe.';

  @override
  String get sendCode => 'Envoyer le code';

  @override
  String get rememberPassword => 'Vous vous souvenez de votre mot de passe ?';

  @override
  String get signInWithGoogle => 'Se connecter avec Google';

  @override
  String get verifyYourEmail => 'Vérifiez votre e-mail';

  @override
  String get otpSentTo =>
      'Nous avons envoyé un code de vérification à 4 chiffres à';

  @override
  String get accountVerifiedSuccess => 'Compte vérifié avec succès !';

  @override
  String get didntReceiveCode => 'Vous n\'avez pas reçu le code ? Renvoyer';

  @override
  String get resending => 'Renvoi en cours...';

  @override
  String get enterVerificationCode => 'Entrez le code de vérification';

  @override
  String get weSentCodeTo => 'Nous avons envoyé un code à 4 chiffres à';

  @override
  String get verifyCode => 'Vérifier le code';

  @override
  String resendCodeIn(Object seconds) {
    return 'Renvoyer le code dans $seconds secondes';
  }

  @override
  String get createNewPassword => 'Créer un nouveau mot de passe';

  @override
  String get newPasswordInstructions =>
      'Votre nouveau mot de passe doit être différent des mots de passe précédemment utilisés.';

  @override
  String get passwordResetSuccess => 'Mot de passe réinitialisé avec succès !';

  @override
  String get usernameRequired => 'Le nom d\'utilisateur est requis';

  @override
  String get usernameMinLength =>
      'Le nom d\'utilisateur doit comporter au moins 3 caractères';

  @override
  String get emailOrUsernameRequired =>
      'L\'e-mail ou le nom d\'utilisateur est requis';

  @override
  String get passwordRequired => 'Le mot de passe est requis';

  @override
  String get passwordStrengthError =>
      'Le mot de passe doit comporter au moins 8 caractères avec majuscule, minuscule, chiffre et caractère spécial';
}
