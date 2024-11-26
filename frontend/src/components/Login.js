import React, { useState } from "react";
import axios from "axios";
import { useNavigate } from "react-router-dom";

const Login = ({ setToken }) => {
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const navigate = useNavigate();

    const handleSubmit = async (e) => {
        e.preventDefault();

        try {
            const response = await axios.post("http://localhost:5290/api/account/login", {
                email,
                password,
            });

            const token = response.data.token;
            localStorage.setItem("token", token);
            setToken(token);
            alert("Login successful!");

            const accountType = response.data.accountType;
            if (accountType === "Customer") {
                navigate.push("/customer");
            } else if (accountType === "Restaurant") {
                navigate.push("/restaurant");
            } else if (accountType === "DeliveryDriver") {
                navigate.push("/deliveryDriver");
            } else if (accountType === "Admin") {
                navigate.push("/admin");
            }
        } catch (error) {
            alert("Login failed: " + error.message);
        }
    };

    return (
        <div>
            <h2>Login</h2>
            <form onSubmit={handleSubmit}>
                <input
                    type="email"
                    placeholder="Email"
                    value={email}
                    onChange={(e) => setEmail(e.target.value)}
                    required
                />
                <input
                    type="password"
                    placeholder="Password"
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                    required
                />
                <button type="submit">Login</button>
            </form>
        </div>
    );
};

export default Login;
