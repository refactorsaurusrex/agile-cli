# AgileCli

AgileCli is a command line tool for generating Agile metrics from Jira data, including:

- Average velocity
- Average number of story points committed
- Average number of story points that rolled over
- Average number of unplanned story points
- Percent of sprints where the number of points completed were within an acceptable margin of error 
- Absolute number of points committed, completed, rolled over, and unplanned each sprint.

Unlike Jira's web-based reports, AgileCli allows further manipulation of all this data via any standard PowerShell function. If you really want to go crazy, you can easily export the data to a CSV, open it in Excel, and really go to town!

## Getting Started

### System Requirements

AgileCli is cross-platform and will work on Windows, Mac, and Linux. The only hard requirement is [PowerShell](https://github.com/PowerShell/PowerShell/releases/latest) 6 or later. 

### Installation

Install from the PowerShell Gallery:

```powershell
Install-Module AgileCli
```

Once it's installed, the module will load automatically whenever a new PowerShell instance is created. To load it immediately after installation, just run `Import-Module AgileCli`. 

### Initial Setup

Before you can use AgileCli, there's some initial setup that must be done:

1. Log into your Jira account and generate an [API token](https://id.atlassian.com/manage-profile/security/api-tokens). 
2. Run `Save-JiraToken` and follow the prompts. This will securely store your API token on your local machine. Note that you cannot use keyboard shortcuts to paste your token. Instead, right click to paste. (PowerShell is awesome, but it's not perfect, and this is one of its more annoying limitations.)
3. Run `Edit-AgileCliConfiguration`. This will load your AgileCli configuration file in your default yaml file editor. Enter your Jira instance's hostname where indicated. Do not include "https://". Instead, use the format "my-company.atlassian.net". Save the file and close. (For more information on the other configuration settings, see [this](https://github.com/refactorsaurusrex/agile-cli/wiki/Edit-AgileCliConfiguration).)
4. Now you're ready for action! Read on...

### A Quick Overview

> For more details on each AgileCli cmdlet, check out [the wiki](https://github.com/refactorsaurusrex/agile-cli/wiki). 

The first thing you'll need is the name of the Jira board you want to use for data. Run `Get-JiraBoards` to get a list of all available boards. You can filter them by running something like `Get-JiraBoards | ?{$_.Name -like "*MyBoard*"}`. Now that you have your board name, you can start running reports.  Here are some examples:

#### Sprint History

```powershell
PS> Get-SprintHistory -BoardName "Team Awesome" | ft

Name                           Started               Ended                 Committed Unplanned Completed Rollover PctRollover
----                           -------               -----                 --------- --------- --------- -------- --------------
White Sands Walrus             9/1/2020 12:04:51 PM  9/15/2020 11:08:46 AM        33         0        24        9 27%
Voyageurs Vampire Squid        8/18/2020 11:57:10 AM 9/1/2020 10:48:45 AM         31         0        28        3 10%
Uwharrie Unicornfish           8/4/2020 12:08:44 PM  8/18/2020 11:09:08 AM        38         1        21       17 45%
Theodore Roosevelt Tiger Shark 7/21/2020 2:27:48 PM  8/4/2020 9:58:11 AM          45         8        29       16 36%
Shenandoah SarcasticFringehead 7/7/2020 12:27:22 PM  7/21/2020 11:02:00 AM        33         0        30        3 9%
Redwood Requiem Shark          6/23/2020 2:14:16 PM  7/7/2020 10:35:51 AM         35         5        30        5 14%
Qausuittuq Quahog              6/9/2020 11:31:12 AM  6/23/2020 10:44:43 AM        30         0        25        5 17%
Pinnacles Plankton             5/26/2020 9:34:50 AM  6/9/2020 10:44:16 AM         34         7        27        7 21%
Olympic Otter                  5/12/2020 12:29:27 PM 5/26/2020 11:31:41 AM        31         0        25        6 19%
Nantahala Nudibranch           4/28/2020 12:13:31 PM 5/12/2020 9:54:32 AM         45         0        36        9 20%
```

#### Project Velocity

```powershell
PS> Get-ProjectVelocity -BoardName "Team Awesome"

AverageCommitted AverageCompleted AverageRollover AverageUnplanned
---------------- ---------------- --------------- ----------------
              35               27               8                2
```

#### Percent On Target Sprints

````powershell
PS> Get-PercentOnTargetSprints -BoardName "Team Awesome"

30%
````

#### Configurations

You can set a default board name, so you don't have to type use the `-BoardName` every single time. To set the default, run `Edit-AgileCliConfiguration` and enter the default board name where indicated. You can always override the default by using the `-BoardName` parameter.

The `Get-PercentOnTargetSprints` cmdlet relies on a setting called `SprintTargetPercent`. This is the maximum allowable variance between sprint commitment and completion in order to still be considered "on target". The default target is 15%, meaning as long as the completed number of points in each sprint is within +/-15% of the commitment, the team is still considered on target. You can change this target to any value between 1 and 100 by running `Edit-AgileCliConfiguration`.

## Help

1. Run `help <cmdlet_name> ` to view help documentation in your terminal window. Example: `help Get-JiraBoards`.
2. Read the [wiki documentation](https://github.com/refactorsaurusrex/agile-cli/wiki). 
3. If all else fails, [open an issue](https://github.com/refactorsaurusrex/agile-cli/issues). 