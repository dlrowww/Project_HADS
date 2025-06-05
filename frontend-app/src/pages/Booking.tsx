import React, { useState } from 'react';

const Booking: React.FC = () => {
    const [date, setDate] = useState('');
    const [service, setService] = useState('');
    const [error, setError] = useState('');

    const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault();
        if (!date || !service) {
            setError('Please select a date and service.');
            return;
        }
        // Handle booking logic here
        console.log('Booking made for:', { date, service });
        setError('');
    };

    return (
        <div className="booking-container">
            <h1>Make a Booking</h1>
            {error && <p className="error">{error}</p>}
            <form onSubmit={handleSubmit}>
                <div>
                    <label htmlFor="date">Select Date:</label>
                    <input
                        type="date"
                        id="date"
                        value={date}
                        onChange={(e) => setDate(e.target.value)}
                    />
                </div>
                <div>
                    <label htmlFor="service">Select Service:</label>
                    <select
                        id="service"
                        value={service}
                        onChange={(e) => setService(e.target.value)}
                    >
                        <option value="">--Select a Service--</option>
                        <option value="service1">Service 1</option>
                        <option value="service2">Service 2</option>
                        <option value="service3">Service 3</option>
                    </select>
                </div>
                <button type="submit">Book Now</button>
            </form>
        </div>
    );
};

export default Booking;