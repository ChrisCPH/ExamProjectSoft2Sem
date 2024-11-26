import React, { useState } from "react";
import axios from "axios";

const CreateAccount = () => {
  const [name, setName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [phoneNumber, setPhoneNumber] = useState("");
  const [language, setLanguage] = useState("");
  const [accountType, setAccountType] = useState(0);
  const [error, setError] = useState(null);

  const handleSubmit = async (e) => {
    e.preventDefault();

    const newAccount = {
      name: name,
      email: email,
      password: password,
      phoneNumber: phoneNumber,
      language: language,
      accountType: accountType,
    };

    try {
      const response = await axios.post("http://localhost:5290/api/account/create", newAccount);
      console.log("Account created successfully:", response.data);
    } catch (err) {
      setError("An error occurred while creating the account.");
      console.error("Error creating account:", err);
    }
  };

  return (
    <div className="container">
      <h2>Create an Account</h2>
      {error && <div className="error">{error}</div>}
      <form onSubmit={handleSubmit}>
        <div>
          <label htmlFor="name">Name</label>
          <input
            type="text"
            id="name"
            value={name}
            onChange={(e) => setName(e.target.value)}
            required
          />
        </div>
        <div>
          <label htmlFor="email">Email</label>
          <input
            type="email"
            id="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            required
          />
        </div>
        <div>
          <label htmlFor="password">Password</label>
          <input
            type="password"
            id="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
          />
        </div>
        <div>
          <label htmlFor="phoneNumber">Phone Number</label>
          <input
            type="text"
            id="phoneNumber"
            value={phoneNumber}
            onChange={(e) => setPhoneNumber(e.target.value)}
            required
          />
        </div>
        <div>
          <label htmlFor="language">Language</label>
          <input
            type="text"
            id="language"
            value={language}
            onChange={(e) => setLanguage(e.target.value)}
            required
          />
        </div>
        <div>
          <label htmlFor="accountType">Account Type</label>
          <select
            id="accountType"
            value={accountType}
            onChange={(e) => setAccountType(Number(e.target.value))}
          >
            <option value={0}>Customer</option>
            <option value={1}>Restaurant</option>
            <option value={2}>DeliveryDriver</option>
            <option value={3}>Admin</option>
          </select>
        </div>
        <button type="submit">Create Account</button>
      </form>
    </div>
  );
};

export default CreateAccount;
