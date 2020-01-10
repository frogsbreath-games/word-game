import * as React from "react";
import Logo from "../assets/FrogsBreath.png";

const LogoFooter = () => {
  return (
    <div>
      <div
        style={{
          textAlign: "center",
          position: "absolute",
          bottom: 0,
          width: "100%"
        }}
      >
        <img src={Logo} width="150px" alt="FrogsBreath Logo" />
        <p>© 2019 FrogsBreath Games</p>
      </div>
    </div>
  );
};

export default LogoFooter;
