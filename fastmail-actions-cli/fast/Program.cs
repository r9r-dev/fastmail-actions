// See https://aka.ms/new-console-template for more information

using fast.Services;

// ask user for description
Console.WriteLine("Description pour l'email:");
var description = Console.ReadLine();
var service = new FastmailService();
if (description != null)
{
    var email = await service.SetMaskedEmail(description);
    Console.WriteLine($"Email créé : {email}");
    // copy email to clipboard
    TextCopy.ClipboardService.SetText(email);
    Console.WriteLine("L'email a été copié dans le presse-papier.");
}
else
{
    Console.WriteLine("Veuillez entrer une description.");
}

// wait for user input
Console.WriteLine("Appuyez sur une touche pour quitter.");
Console.ReadKey();