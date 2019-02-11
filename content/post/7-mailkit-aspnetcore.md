---
title: "Mailkit ASP.NET Core"
date: 2018-08-27T09:57:44+10:00
draft: false
type: "post"
slug: "mailkit-aspnetcore"
---

[Mailkit](https://github.com/jstedfast/MailKit) is the new official replacement for the {{< inline "SMTPClient">}} as indicated [here](https://www.infoq.com/news/2017/04/MailKit-MimeKit-Official).
There are [much](https://dotnetcoretutorials.com/2017/11/02/using-mailkit-send-receive-email-asp-net-core/) better tutorials than anything I can write right now, but here's a quick primer (there's also a good tutorial in the Readme for Mailkit)

- Install the [nuget package](https://www.nuget.org/packages/MailKit/) to your project.
- Use.  

Nuget is nice sometimes.  
The usage is also fairly straightforward. Below is a generalised snippit for SENDING an email and the tutorials above cover both sending and receiving. 

``` csharp
var message = new MimeMessage();
message.To.AddRange(emailMessage.To.Select(t => new MailboxAddress(t.Name, t.Address)));
message.From.AddRange(emailMessage.From.Select(f => new MailboxAddress(f.Name, f.Address)));
message.Subject = emailMessage.Subject;

message.Body = new TextPart(TextFormat.Text)
{
    Text = emailMessage.Content
};

using (var emailClient = new SmtpClient())
{
    emailClient.Connect(_emailConfiguration.SmtpServer, _emailConfiguration.SmtpPort, true);
    emailClient.AuthenticationMechanisms.Remove("XOAUTH2");
    emailClient.Authenticate(_emailConfiguration.SmtpUserName, _emailConfiguration.SmtpPassword);
    emailClient.Send(message);
    emailClient.Disconnect(true);
}
```  

{{< inline "_emailConfiguration" >}} is just a config dump and makes environment changes easy.