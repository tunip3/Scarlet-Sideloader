# Scarlet-Sideloader-GUI
Scarlet Sideloader GUI is as the name implies a GUI version of the Scarlet Sideloader tool designed to help you push apps to retail via the store.
![image](https://user-images.githubusercontent.com/26260613/224582532-e4c0f018-c7e2-480d-a7b9-98b44336e02d.png)

This will only push apps to the store with randomised names and will only allow you to push private apps to selected groups.
The names are randomised because Microsoft has started to detect apps based on their names and apps are private because private apps are harder to detect.
When private apps and randomised names are combined it is harder for Microsoft to detect the apps so as long as you remain responsible your account should take longer to get deactivated. Private apps also come with a secondary benefit of showing the package name rather than the store name once installed, this means that Retroarch once installed will still show up as Retroarch.

## Support
[Creating User Groups](#creating-user-groups)


### Creating User Groups
In order to use private apps you must use what is known as "Known User Groups". A "Known User Group" is a group of emails defined in partner center which you can grant access to a private app, only accounts with their email address in this group will be able to see the app once it passes certification.

1. Navigate to the [Customer Groups Portion of Partner center](https://partner.microsoft.com/en-us/dashboard/analytics/customers)
![image](https://user-images.githubusercontent.com/26260613/224698318-fcc9cead-284c-4bad-b137-191668e6d240.png)

2. Select Create New Group
![image](https://user-images.githubusercontent.com/26260613/224698399-16c88b51-2d1a-47fc-917c-b1550bea1497.png)

3. Make sure that "Known User Group" is selected ![image](https://user-images.githubusercontent.com/26260613/224698930-470e53b8-a156-4d5a-a986-5865eae50297.png)

4. Fill in your Email addresses. 
![image](https://user-images.githubusercontent.com/26260613/224699089-32a85909-0efb-4239-b3a3-75aa15ec0c41.png)

5. Select Save
![image](https://user-images.githubusercontent.com/26260613/224699284-4963327c-cb2e-4b12-87a4-96f291d24b3a.png)

6. Login to the sideloader and select your target group

![image](https://user-images.githubusercontent.com/26260613/224699782-d6792e6c-c9c3-42b6-a856-6556ecec33bc.png)



### Obtaining a developer account
If you do not already have a developer account, obtaining one is fairly easily and can be done through [Microsoft's official web page](https://partner.microsoft.com/dashboard/registration)

You should select the individual account option when signing up.

You may be able to get a cheaper account by creating an account in another region.
If you want to go down this route you can find a spreadsheet on what region is currently the cheapest [here](https://docs.google.com/spreadsheets/d/1uwcU4AoTbC-8Of3ukC6Mut8_EwvPbrWlL94dbTo2wV4/edit?usp=drivesdk).
Note that your partner centre account region may be different to your main account region.
When signing up for an account in a different region you should use fake information such as a McDonalds address in the region for everything except the billing information.


### Getting your .AspNet.Cookies

For the .AspNet.Cookies you can use [partner token](https://github.com/Dantes-Dungeon/PartnerToken/tree/054d5e0154d32de86e44ed877f575002d5e90f53) or get it manually.

#### Manually getting .AspNET.Cookies:

1. Open [Microsoft Partner Centre](https://partner.microsoft.com/en-us/dashboard/apps-and-games/overview)
![image](https://user-images.githubusercontent.com/26260613/224584000-67b44326-2675-4266-bd9d-8631c8ef23bd.png)

2. Open the developer tools (ctrl-shift-i in edge) and select Application
![image](https://user-images.githubusercontent.com/26260613/224584093-1a37308c-d023-43a0-98c6-b69bd75f9004.png)

3. Select Cookies 
![image](https://user-images.githubusercontent.com/26260613/224584165-0e6cdfa1-4d88-4cbf-a2eb-2d69982d2e2d.png)

4. Select Partner Centre in Cookies
![image](https://user-images.githubusercontent.com/26260613/224584276-bf7ecf31-5331-49e3-a7b3-115a75039058.png)

5. Select .AspNet.Cookies
![image](https://user-images.githubusercontent.com/26260613/224584356-2c584f74-6b74-4f66-a1d6-7569766d5165.png)

6. Copy the Cookie Value
![image](https://user-images.githubusercontent.com/26260613/224584477-ed1352c2-7d59-449e-bf27-ad3df38eeb68.png)

7. Paste this into Scarlet Sideloader

![image](https://user-images.githubusercontent.com/26260613/224584513-b322ebc2-6bc6-462e-a12e-ffd38b1b7ce7.png)


