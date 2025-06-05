import React from 'react';
import { BrowserRouter as Router, Route, Switch } from 'react-router-dom';
import Login from './pages/Login';
import SignUp from './pages/SignUp';
import User from './pages/User';
import Booking from './pages/Booking';
import Payment from './pages/Payment';
import History from './pages/History';

const App: React.FC = () => {
    return (
        <Router>
            <Switch>
                <Route path="/login" component={Login} />
                <Route path="/signup" component={SignUp} />
                <Route path="/user" component={User} />
                <Route path="/booking" component={Booking} />
                <Route path="/payment" component={Payment} />
                <Route path="/history" component={History} />
                <Route path="/" exact component={Login} />
            </Switch>
        </Router>
    );
};

export default App;