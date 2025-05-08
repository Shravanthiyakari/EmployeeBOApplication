namespace EmployeeBOApp.EmailsContent
{
    public class EmailContentforRDRequestChange
    {
        public static string EmailContentForRDC =
             @"
          <!DOCTYPE html>
          <html>
          <head>
          <style>
              body {
              font-family: Arial, sans-serif;
              line-height: 1.6;
              color: #333;
              }
          .container {
              padding: 10px;
           }
          .table {
              border-collapse: collapse;      
              width: 100%;
              margin-top: 10px;
              margin-bottom: 20px;
           }
          .table th, .table td {
           border: 1px solid #ccc;
           padding: 8px 12px;
           text-align: left;
          }
           .footer {
          font-size: 12px;    
          color: #777;
          margin-top: 20px;
          }
          </style>
          </head>
          <body>
<div class='container'>
<p>Hi,</p>
<p>Request has been {Request_Status} By {user_name}</p>
<p>Request you to update Dynamics 365 with the following changes.</p>

<p><strong>Change Request:</strong></p>
<table class='table'>
<tr>
  <th>Emp ID</th>
  <th>Name</th>
  <th>Project Code</th>
  <th>Project Name</th>
  <th>Department ID</th>
  <th>Reporting Manager</th>
  <th>Start Date</th>
</tr>
<tr>
  <td>{EMP_ID}</td>
  <td>{EMP_NAME}</td>
  <td>{PROJECT_CODE}</td>
  <td>{PROJECT_Name}</td>
  <td>{DEPARTMENT_ID}</td>
  <td>{ReportingManager}</td>
  <td>{START_DATE}</td>
</tr>
</table>
{ACTION_LINK}

<p>Thanks.</p>
<p class='footer'>This is an automated generated email. Please do not reply to this email.</p>
</div>
</body>
</html>";
    }
}