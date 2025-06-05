import React, { useEffect, useState } from 'react';

const History: React.FC = () => {
    const [bookings, setBookings] = useState([]);

    useEffect(() => {
        // Fetch the user's booking history from an API or local storage
        const fetchBookingHistory = async () => {
            try {
                const response = await fetch('/api/bookings'); // Replace with your API endpoint
                const data = await response.json();
                setBookings(data);
            } catch (error) {
                console.error('Error fetching booking history:', error);
            }
        };

        fetchBookingHistory();
    }, []);

    return (
        <div>
            <h1>Your Booking History</h1>
            {bookings.length === 0 ? (
                <p>No bookings found.</p>
            ) : (
                <ul>
                    {bookings.map((booking) => (
                        <li key={booking.id}>
                            <p>Booking ID: {booking.id}</p>
                            <p>Date: {booking.date}</p>
                            <p>Service: {booking.service}</p>
                        </li>
                    ))}
                </ul>
            )}
        </div>
    );
};

export default History;