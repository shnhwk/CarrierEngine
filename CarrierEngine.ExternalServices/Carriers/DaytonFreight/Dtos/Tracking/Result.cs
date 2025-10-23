using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CarrierEngine.ExternalServices.Carriers.DaytonFreight.Dtos.Tracking;

public class Result
{
    [JsonPropertyName("status")]
    public Status Status { get; set; }

    [JsonPropertyName("shipper")]
    public Shipper Shipper { get; set; }

    [JsonPropertyName("consignee")]
    public Consignee Consignee { get; set; }

    [JsonPropertyName("originServiceCenter")]
    public OriginServiceCenter OriginServiceCenter { get; set; }

    [JsonPropertyName("destinationServiceCenter")]
    public DestinationServiceCenter DestinationServiceCenter { get; set; }

    [JsonPropertyName("originPartner")]
    public object OriginPartner { get; set; }

    [JsonPropertyName("destinationPartner")]
    public DestinationPartner DestinationPartner { get; set; }

    [JsonPropertyName("billOfLadingNumbers")]
    public List<string> BillOfLadingNumbers { get; set; }

    [JsonPropertyName("purchaseOrders")]
    public List<string> PurchaseOrders { get; set; }

    [JsonPropertyName("shipperNumbers")]
    public List<string> ShipperNumbers { get; set; }

    [JsonPropertyName("details")]
    public List<Detail> Details { get; set; }

    [JsonPropertyName("pro")]
    public string Pro { get; set; }

    [JsonPropertyName("terms")]
    public string Terms { get; set; }

    [JsonPropertyName("entryDate")]
    public DateTime? EntryDate { get; set; }

    [JsonPropertyName("pickupDate")]
    public DateTime? PickupDate { get; set; }

    [JsonPropertyName("estimatedDeliveryDate")]
    public DateTime? EstimatedDeliveryDate { get; set; }

    [JsonPropertyName("isAppointment")]
    public bool IsAppointment { get; set; }

    [JsonPropertyName("deliveryDate")]
    public DateTime? DeliveryDate { get; set; }

    [JsonPropertyName("handlingUnits")]
    public int HandlingUnits { get; set; }

    [JsonPropertyName("weight")]
    public int Weight { get; set; }

    [JsonPropertyName("charges")]
    public double Charges { get; set; }

    [JsonPropertyName("discount")]
    public double Discount { get; set; }
}