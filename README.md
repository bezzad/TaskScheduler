# Task Scheduler

Dynamic Task Scheduler as windows service

# How to work

For install Task Scheduler service on windows, run it as Administrator CMD and run the these codes:

`$ TaskScheduler.exe install start`

or execute `Install.bat` file in from root folder.
And for Uninstall app:

```
$ sc stop TaskScheduler
$ sc delete TaskScheduler
```

or execute `Uninstall.bat` file in from root folder.

# How to Create Slack WebhookUrl

Please see [this helpful](https://get.slack.help/hc/en-us/articles/115005265063-Incoming-WebHooks-for-Slack) address to create your own incoming WebHooks for Slack.

# How to add jobs and notification services to setting file

Please change the `TaskSchedulerSetting.json` file as well as for what you want to execute and notifying.
This file have 3 part:

* list of your jobs
* list of default notifications receivers
* list of notification services

For example of the below setting, you can seen that slack notification service defined in `notificationServices` part and by type `SlackService` and name `testSlack` which called from `notifications` part for service provider.

And in `notifications` part used `SlackService` for `alerts` channel as notification receiver. This part can defined notifications to all jobs as deault receiver, but if a job have self notification definition, so just will send notify to that.

``` 

{
  "jobs": [
    {
      "jobType": "Recurring",
      "enable": true,
      "name": "purchese checker (method: bpm)",
      "description": "Call purchese web api every hour",
      "actionName": "CallRestApi",
      "actionParameters": {
        "url": "http://localhost:8002/v1/monitor/payment",
        "httpMethod": "GET",
        "queryParameters": {
          "duration": 60,
          "min": 0,
          "method": "bpm"
        }
      },
      "notifyCondition": "NotEquals",
      "notifyConditionResult": "OK: \"\"",
      "triggerOn": "0 * * * *", // every hour
      "notifications": [
        {
          "notificationServiceName": "RahyabSmsService",
          "receiver": "09354028149;"
        }
      ]
    },
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
      "name": "download checker",
      "description": "Call download web api every 30 minute",
      "actionName": "CallRestApi",
      "actionParameters": {
        "url": "http://localhost:8002/v1/monitor/download",
        "httpMethod": "GET",
        "queryParameters": {
          "duration": 30,
          "value": 4.0
        }
      },
      "notifyCondition": "NotEquals",
      "notifyConditionResult": "OK: \"\"",
      "triggerOn": "*/30 * * * *" // every 30 minutes
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
      "triggerOn": "0 */2 * * *" // every 120 min
    },
    {
      "jobType": "Recurring",
      "enable": true,
      "name": "RabbitMQ uptime Checker",
      "description": "Check rabbitmq uptime by sending sample data",
      "actionName": "CallRestApi",
      "actionParameters": {
        "url": "http://localhost:8002/v1/monitor/rabbitmq",
        "queryParameters": {
        },
        "httpMethod": "GET"
      },
      "notifyCondition": "NotEquals",
      "notifyConditionResult": "OK: \"\"",
      "triggerOn": "*/5 * * * *" // every 5 min
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
      "receiver": "12345678, @notification_channel"
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
      "logoUrl": null,
      "clickUrl": "https://test.com",
      "unsubscribeUrl": "http://api.test.com/unsubscribe/?mail={receiver}",
      "isDefaultService": true
    },
    {
      "serviceName": "testTelegram",
      "notificationType": "TelegramService",
      "username": "@notification_bot",
      "apiKey": "012345678:XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX",
      "senderBot": "@notification_bot",
      "isDefaultService": true
    },
    {
      "serviceName": "testSlack",
      "notificationType": "SlackService",
      "senderName": "TaskScheduler {#version}",
      "webhookUrl": "https://hooks.slack.com/services/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",
      "iconUrl": ":robot_face:",
      "isDefaultService": true
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