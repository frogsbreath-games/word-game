import * as React from "react";
import { Container } from "reactstrap";
import NavMenu from "./NavMenu";
import LogoFooter from "./LogoFooter";

export default (props: { children?: React.ReactNode }) => (
  <React.Fragment>
    <NavMenu />
    {/* this to keep the footer in place */}
    <div style={{ position: "relative", minHeight: "100vh" }}>
      <div style={{ paddingBottom: "16rem" }}>
        <Container>{props.children}</Container>
      </div>
      <LogoFooter />
    </div>
  </React.Fragment>
);
