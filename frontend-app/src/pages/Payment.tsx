import React, { useState } from 'react';

const Payment: React.FC = () => {
    const [amount, setAmount] = useState('');
    const [cardNumber, setCardNumber] = useState('');
    const [expiryDate, setExpiryDate] = useState('');
    const [cvv, setCvv] = useState('');
    const [error, setError] = useState('');
    const [success, setSuccess] = useState(false);

    const handlePayment = (e: React.FormEvent) => {
        e.preventDefault();
        // Validate input fields
        if (!amount || !cardNumber || !expiryDate || !cvv) {
            setError('All fields are required');
            return;
        }
        // Process payment logic here
        // For demonstration, we'll just simulate a successful payment
        setSuccess(true);
        setError('');
    };

    return (
        <div className="payment-container">
            <h2>Payment</h2>
            {success ? (
                <div className="success-message">Payment Successful!</div>
            ) : (
                <form onSubmit={handlePayment}>
                    {error && <div className="error-message">{error}</div>}
                    <div>
                        <label>Amount:</label>
                        <input
                            type="text"
                            value={amount}
                            onChange={(e) => setAmount(e.target.value)}
                        />
                    </div>
                    <div>
                        <label>Card Number:</label>
                        <input
                            type="text"
                            value={cardNumber}
                            onChange={(e) => setCardNumber(e.target.value)}
                        />
                    </div>
                    <div>
                        <label>Expiry Date:</label>
                        <input
                            type="text"
                            placeholder="MM/YY"
                            value={expiryDate}
                            onChange={(e) => setExpiryDate(e.target.value)}
                        />
                    </div>
                    <div>
                        <label>CVV:</label>
                        <input
                            type="text"
                            value={cvv}
                            onChange={(e) => setCvv(e.target.value)}
                        />
                    </div>
                    <button type="submit">Pay Now</button>
                </form>
            )}
        </div>
    );
};

export default Payment;