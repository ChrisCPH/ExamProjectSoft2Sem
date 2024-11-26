import React, { useState, useEffect } from "react";
import { BrowserRouter as Router, Route, Routes } from "react-router-dom";
import AccountCreation from "./components/CreateAccount";
import Login from "./components/Login";
import Navbar from "./components/Navbar";
import CustomerPage from "./pages/CustomerPage";
import RestaurantPage from "./pages/RestaurantPage";
import DeliveryDriverPage from "./pages/DeliveryDriverPage";
import AdminPage from "./pages/AdminPage";
import { jwtDecode } from "jwt-decode";
import ProtectedRoute from "./components/ProtectedRoute";
import "./css/styles.css";
import "./css/navbar.css"; 

const App = () => {
  const [token, setToken] = useState(localStorage.getItem("token"));
  const [accountType, setAccountType] = useState(null);

  useEffect(() => {
    if (token) {
      try {
        const decodedToken = jwtDecode(token);
        setAccountType(decodedToken.AccountType);
      } catch (err) {
        console.error("Invalid token:", err);
        localStorage.removeItem("token");
        setToken(null);
      }
    }
  }, [token]);

  return (
    <Router>
      <Navbar accountType={accountType} />
      <Routes>
        <Route path="/register" element={<AccountCreation />} />
        <Route path="/login" element={<Login setToken={setToken} />} />

        <Route
          path="/customer"
          element={
            <ProtectedRoute
              component={CustomerPage}
              allowedRoles={["Customer"]}
              accountType={accountType}
            />
          }
        />
        <Route
          path="/restaurant"
          element={
            <ProtectedRoute
              component={RestaurantPage}
              allowedRoles={["Restaurant"]}
              accountType={accountType}
            />
          }
        />
        <Route
          path="/deliveryDriver"
          element={
            <ProtectedRoute
              component={DeliveryDriverPage}
              allowedRoles={["DeliveryDriver"]}
              accountType={accountType}
            />
          }
        />
        <Route
          path="/admin"
          element={
            <ProtectedRoute
              component={AdminPage}
              allowedRoles={["Admin"]}
              accountType={accountType}
            />
          }
        />
      </Routes>
    </Router>
  );
};

export default App;
