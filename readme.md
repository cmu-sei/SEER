# SEER: System (for) Event Evaluation Research

![](docs/images/icon.png)

## Project Background

SEER automates the collection and processing of training and exercise data — from participant-entered incident response case reports to collaborative chat to other range systems — to provide detailed assessment-related reports on team and individual performance.

By providing qualitative & quantitative analysis of performance and removing subjectivity, its results enable the refinement of regular best practices and subsequent adaptation to T&R standards. SEER assists in the identification of high-performing units within training and exercise. It is a step to remove "game-isms" within an assessment by enabling participants to self-report their observations and subsequent activity.

- [Build and install guide](docs/install.md)

- [Quickstart guide](docs/quickstart.md)

## Overview

SEER, in combination with an IR platform (TheHive) and Communications App (Mattermost), can capture all three essential data points in real time and provide reports on and comparisons between teams exercising under the same scenarios. SEER collects all data from TheHive and Mattermost and maps messages and actions to associated teams and users, it also tracks progress of Incident Response for each scheduled inject (incident) within the exercise. From this, SEER produces individual and team reports on the actions taken within the exercise, and provides timelines of the IR process for each inject.

## Team Assessment Challenges

The ideal assessment would involve analyzing every step of a unit’s process for incident management (for DCO, this is identification, mitigation, quarantine, etc.), the timing related to their action, and their lines of communication as they operate. These requirements have been hard to capture with traditional assessment systems.

There are many stakeholders with distinct needs within exercise and training, including:

- Exercise Administrators
- White Cell or Embedded Observers
- Red Team or OPFOR
- Blue Team or DCO

## Problem Statement

The SEER project aspires to help solve some of the perennial challenges in evaluating individual and team performance within a cyber exercise:

1. Clearly identify high performers
2. Conduct qualitative & quantitative analysis on indicators/drivers of that high performance
3. Use collected data to establish regular best-practice refinement in training and exercise standards
4. Determine why high performing individuals and teams did better
5. Survey a team's organizational characteristics, composition, and task performance
6. Analyze indicators/drivers of high performance over time, including:
   - Organizational characteristics
   - Team composition
   - Team & individual task performance

As part of ongoing evaluation, SEER seeks to answer assessment-related questions such as the following:

- What do we assess?
- Who is being assessed?
- How do we measure each assessment item?
- Which data is needed for that assessment item? Qualitative/Quantitative
- Reporting

## Typical Workflow

1. Admins/OPFOR design exercise scenario along a timeline, including individual injects with each step mapped to MITRE ATT&CK
   - Injects are evolved from our internal inject catalog of hundreds of exploits
2. Admin/OPFOR clicks button in SEER to add event to MISP with default mapped info, which includes necessary tags for when it comes back to SEER. (MISP also provides HHQ intel on potential threats)
3. MISP automatically updates HIVE. 
4. As the assessed team provides updates from within The Hive on activity they are seeing, SEER consumes this data and processes it to make an effective assessment

## Next Steps

Future projects include the following:

### Leverage Framework of Frameworks

We are looking to integrate popular frameworks such as:

- CyberDEM
- NIST NICE
- NIST CSF
- ATT&CK
