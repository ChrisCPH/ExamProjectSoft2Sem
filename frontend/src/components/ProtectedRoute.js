import React from "react";
import { Navigate } from "react-router-dom";

const ProtectedRoute = ({ component: Component, allowedRoles, accountType }) => {
  if (!accountType || !allowedRoles.includes(accountType)) {
    return <Navigate to="/login" replace />;
  }

  return <Component />;
};

export default ProtectedRoute;
