namespace EmployeeBOApp.EmailContent
{
    public class EmailContentForBGVID
    {
        public static string EmailContentBGVID =
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
<p>Dear Team,</p>
<p>A Background Verification (BGV) ID request has been completed for the following employee: </p>
<p><strong>Full Name:</strong> {EMP_NAME}</p>
<p><strong>Employee ID:</strong> {EMP_ID}</p>
<p><strong>BGV ID:</strong> {BGV_ID}</p>

<p class='footer'>This is an automated generated email. Please do not reply to this email.</p>

<p>Best Regards</p>
<p>{HR_NAME}</p>
</div>
</body>
</html>";
    }
}