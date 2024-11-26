import React, { useState, useEffect } from "react";
import { Link } from "react-router-dom";
import { jwtDecode } from "jwt-decode";

const Navbar = () => {
  const [accountType, setAccountType] = useState(null);
  const [isAuthenticated, setIsAuthenticated] = useState(false);

  // Retrieve token from localStorage and decode it
  useEffect(() => {
    const token = localStorage.getItem("token");
    if (token) {
      try {
        const decodedToken = jwtDecode(token);
        setAccountType(decodedToken.AccountType); // Ensure the token has an "AccountType"
        setIsAuthenticated(true);
      } catch (error) {
        console.error("Invalid token:", error);
        localStorage.removeItem("token"); // Remove invalid token
        setIsAuthenticated(false);
      }
    } else {
      setIsAuthenticated(false); // If no token is present, set user as not authenticated
    }
  }, []);

  return (
    <nav>
      <ul>
        {!isAuthenticated ? (
          <>
            <li>
              <Link to="/login">Login</Link>
            </li>
            <li>
              <Link to="/register">Register</Link>
            </li>
          </>
        ) : (
          <>
            {accountType === "Customer" && (
              <li>
                <Link to="/customer">Customer Page</Link>
              </li>
            )}
            {accountType === "Restaurant" && (
              <li>
                <Link to="/restaurant">Restaurant Page</Link>
              </li>
            )}
            {accountType === "DeliveryDriver" && (
              <li>
                <Link to="/deliveryDriver">Delivery Driver Page</Link>
              </li>
            )}
            {accountType === "Admin" && (
              <li>
                <Link to="/admin">Admin Page</Link>
              </li>
            )}
          </>
        )}
      </ul>
    </nav>
  );
};

export default Navbar;
