Imports System.Threading.Tasks
Imports Stripe

Public Structure StructStripePaymentReturn
    Public Paid As Boolean
    Public ErrorMessage As String
    Public MerchantFee As Decimal
    Public GST As Decimal
    Public charge As Charge
End Structure

Public Class StripeService
    Public Shared Async Function makeStripeCharge(ByVal AmountToPay As Decimal, ByVal stripeToken As String, ByVal ProductionMode As Boolean, ByVal StripeApiSecretKeyLive As String, ByVal StripeApiSecretKeyDev As String) As Task(Of StructStripePaymentReturn)
        If ProductionMode Then
            StripeConfiguration.ApiKey = StripeApiSecretKeyLive
        Else
            StripeConfiguration.ApiKey = StripeApiSecretKeyDev
        End If

        Dim chargeService = New ChargeService()

        Dim options As ChargeCreateOptions = New ChargeCreateOptions() With {
            .Amount = System.Convert.ToInt32((AmountToPay * CDec(100))),
            .Currency = "aud",
            .Description = "Description to show on receipt sent by Stripe",
            .Source = stripeToken,
            .StatementDescriptor = "Retail Edge" 'Description to show on CC statement (22 chars?)
        }

        Dim charge As Charge = New Charge()

        Try
            charge = Await chargeService.CreateAsync(options)
        Catch ex As Exception
            Dim structStripePaymentReturn As StructStripePaymentReturn
            structStripePaymentReturn.Paid = False
            structStripePaymentReturn.ErrorMessage = ex.Message
            structStripePaymentReturn.charge = charge
            structStripePaymentReturn.MerchantFee = 0
            structStripePaymentReturn.GST = 0
            Return structStripePaymentReturn
        End Try


        If charge.Paid Then
            Dim TotalCharged = charge.Amount / 100
            Dim MerchantFee As Decimal = CDec(0)
            Dim GST As Decimal = CDec(0)

            Dim balanceTransactionService As New BalanceTransactionService
            Dim balanceTransaction As BalanceTransaction = balanceTransactionService.Get(charge.BalanceTransactionId)

            For Each feeDetail In balanceTransaction.FeeDetails
                If feeDetail.Type = "stripe_fee" Then
                    MerchantFee += System.Convert.ToDecimal(feeDetail.Amount) / 100
                End If

                If feeDetail.Type = "tax" Then
                    GST += System.Convert.ToDecimal(feeDetail.Amount) / 100
                End If
            Next

            Dim structStripePaymentReturn As StructStripePaymentReturn
            structStripePaymentReturn.MerchantFee = MerchantFee
            structStripePaymentReturn.GST = GST
            structStripePaymentReturn.Paid = True
            structStripePaymentReturn.ErrorMessage = ""
            structStripePaymentReturn.charge = charge
            Return structStripePaymentReturn
        Else
            Dim structStripePaymentReturn As StructStripePaymentReturn
            structStripePaymentReturn.MerchantFee = 0
            structStripePaymentReturn.GST = 0
            structStripePaymentReturn.charge = charge
            structStripePaymentReturn.Paid = False
            structStripePaymentReturn.ErrorMessage = charge.FailureMessage
            Return structStripePaymentReturn
        End If
    End Function
End Class
