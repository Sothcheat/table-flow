namespace TableFlow.Api.Data.Entities
{
    
        public enum TableStatus
        {
            Available,
            Occupied
        }

        public enum SessionStatus
        {
            Open,
            Closed
        }

        public enum PaymentMethod
        {
            Cash,
            KHQR
        }

        public enum OrderStatus
        {
            Pending,
            Confirmed,
            Preparing,
            Ready,
            Served
        }

        public enum OrderItemStatus
        {
            Waiting,
            Preparing,
            Done,
            Unavailable,
            Delivered
        }
    
}
