# Task Scheduler

Dynamic Task Scheduler as windows service.

This service can send your notifications at a special time or according by resluts of jobs conditions.
You must provide `Jobs`, `Notifications`, `NotificationServices` in the setting file to execute this application.
The setting file's checked every `100 seconds` to set new jobs or remove some services if that changed. So you should wait for 2 minute atleast to see your changed. The application worked based on [Hangfire](https://www.hangfire.io/)and SQL Server.

--------------

## How to work

For install Task Scheduler service on windows, run CMD as administrator and execute the below codes:

`$ TaskScheduler.exe install start`

or run `Install.bat` file in from root folder.
Now you can use the application and enjoy it;

For Uninstall app:

```bash
$ sc stop TaskScheduler
$ sc delete TaskScheduler
```

or run `Uninstall.bat` file in from root folder.

--------------

## How to registering for your WebHook Url with Slack

[Click here to set up an incoming WebHook](https://my.slack.com/services/new/incoming-webhook/)

* Sign in to Slack.
* Choose a channel to post to.
* Then click on the green button `Add Incoming WebHooks integration`.
* You will be given a WebHook Url. Keep this private. Use it when you set up the Slack Notification Service.

--------------

## How to use or create new Telegram notification service

In the `notificationServices` setting you can add your own services like telegram service, for example:

```json
"notificationServices": [
  //...
  {
        "serviceName": "testTelegram", // is notification service name which called from notifications
        "notificationType": "TelegramService", // declare service action type and is must important to have correct type.
        "username": "@NameOfBot",
        "apiKey": "684394290:AAFg3Ht1EtNB7iYe9V_VVxKCEVMP-lSjycA",
        "senderBot": "@NameOfBot",
        "isDefaultService": true // can be used for system notifications
  }
  //...
]
```

* First [create a **Telegram Bot**](https://core.telegram.org/bots#creating-a-new-bot) from [BotFather](https://telegram.me/botfather).

* Get the Bot API token from BotFather. The token is a string along the lines of `110201543:AAHdqTcvCH1vGWJxfSeofSAs0K5PALDsaw` that is required to authorize the bot and send requests to the Bot API.
  
* Set the `apiKey` property of service by api token and set the bot name in `senderBot` property;

Add your bot as administrator user to channel or group which you want to send message or notification (This is not available in Telegram Desktop version);

Enter channel or group name like @testChannel in the notification receiver. for example:

```json
"notifications": [
    //...
    {
      "notificationServiceName": "testTelegram",
      "receiver": "123456789, @testChannel"
    }
    //...
  ],
```

--------------

## How to add jobs and notification services to setting file

Please change the `TaskSchedulerSetting.json` file as well as for what you want to execute and notifying.
This file have 3 part:

* list of your jobs by types:
  * __FireAndForget__ (this jobs are executed only once and almost immediatelyafter creation)
  * __Delayed__ (this jobs are executed only once too, but not immediately, after a certain time interval)
  * __Recurring__ (this jobs fire many times on the specified schedule)
* list of default notifications receivers
* list of notification services by type:
  * __Email__ (this service provide send mail from your special mail service data)
  * __Sms__ (this is a Short Message Service provider)
  * __Telegram__ (this service is for send notifications from a telegram bot to a channel or group which the bot is administrator user member of that)
  * __Slack__ (this service provide a webhook url to send notifications to your slack channels)
  * __CallRestApi__ (is a provider to call or send notifications for your Restfull APIs, must times used for monitoring the APIs)

For example of the below setting, you can see that slack notification service defined in `notificationServices` part and by type `SlackService` and name `testSlack` which called from `notifications` part for service provider.

And in `notifications` part used `SlackService` for `alerts` channel as notification receiver. This part can define notifications to all jobs as deault receiver, but if a job have self notification definition, so just will send notify to that.

```json

{
  "jobs": [
    {
      "jobType": "Recurring",
      "enable": true,
      "name": "max-purchese checker (per hour)",
      "description": "Call purchese web api every hour for max purchase",
      "actionName": "CallRestApi",
      "actionParameters": {
        "url": "http://localhost:8002/v1/monitor/highpayment",
        "httpMethod": "GET",
        "queryParameters": {
          "duration": 60,
          "maxTotal": 200,
          "maxPerUser": 20
        }
      },
      "notifyCondition": "NotEquals",
      "notifyConditionResult": "OK: \"\"",
      "triggerOn": "0 * * * *", // every hour
      "notifications": [
        {
          "notificationServiceName": "testSlack",
          "receiver": "alerts;"
        }
      ]
    },
    {
      "jobType": "Recurring",
      "enable": true,
      "name": "User Account Creates Checker",
      "description": "Check users account create count per minutes",
      "actionName": "CallRestApi",
      "actionParameters": {
        "url": "http://localhost:8002/v1/monitor/account",
        "queryParameters": {
          "duration": 60,
          "min": 0
        },
        "httpMethod": "GET"
      },
      "notifyCondition": "NotEquals",
      "notifyConditionResult": "OK: \"\"",
      "triggerOn": "0 */2 * * *" // every 2 hours
    },
    {
      "jobType": "Recurring",
      "enable": true,
      "name": "log checker",
      "description": "Remove more than 7 days ago files",
      "actionName": "StartProgram",
      "actionParameters": {
        "fileName": "forfiles",
        "arguments": "/p \"C:\\inetpub\\logs\\LogFiles\" /s /m *.* /c \"cmd /c Del @path\" /d -7",
        "windowsStyle": "Hidden",
        "createNoWindow": true,
        "useShellExecute": false
      },
      "triggerOn": "0 12 * * 5" // At 12:00 PM, Only on Friday
    }
  ],
  "notifications": [
    {
      "notificationServiceName": "RahyabSmsService",
      "receiver": "09354028149;09123456789;"
    },
    {
      "notificationServiceName": "taaghcheMailService",
      "receiver": "bezzad@gmail.com;"
    },
    {
      "notificationServiceName": "testSlack",
      "receiver": "alerts"
    },
    {
      "notificationServiceName": "testTelegram",
      "receiver": "106752126, @telester"
    }
  ],
  "notificationServices": [
    {
      "serviceName": "RahyabSmsService",
      "notificationType": "SmsService",
      "username": "1000",
      "password": "TakTak",
      "senderNumber": "50001760",
      "isDefaultService": true
    },
    {
      "serviceName": "taaghcheMailService",
      "notificationType": "EmailService",
      "smtpPassword": "FFFFFFFFFFFFFFFFFFFFFFFFFFFFF",
      "smtpSenderEmail": "postmaster@test.com",
      "smtpUrl": "smtp.mailgun.org",
      "smtpPort": 587,
      "logo": null,
      "logoUrl": "https://a.slack-edge.com/9c217/img/loading_hash_animation.gif",
      "clickUrl": "https://test.com",
      "unsubscribeUrl": "http://api.test.com/unsubscribe/?mail={receiver}",
      "isDefaultService": true
    },
    {
      "serviceName": "testTelegram",
      "notificationType": "TelegramService",
      "username": "@telesterbot",
      "apiKey": "684394290:AAFg3Ht1EtNB7iYe9V_VVxKCEVMP-lSjycA",
      "senderBot": "@telesterbot",
      "isDefaultService": true
    },
    {
      "serviceName": "testSlack",
      "notificationType": "SlackService",
      "senderName": "TaskScheduler {#version}",
      "webhookUrl": "https://hooks.slack.com/services/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",
      "iconUrl": ":robot_face:",
      "isDefaultService": false
    },
    {
      "serviceName": "CallRestApi",
      "notificationType": "CallRestApiService",
      "clientId": "api",
      "clientSecret": "XXXXXXXXXXXXXXXXXXXX",
      "authServerUrl": "https://auth.test.com/connect/token",
      "authGrantType": "client_credentials",
      "authScope": "service",
      "isDefaultService": true
    }
  ]
}

```
