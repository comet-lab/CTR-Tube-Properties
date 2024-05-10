% Data analysis for the experiment performed on 3/19/2024
close all; clear; clc;

% Times
t = [0 0.5 1 1.5 2 4 24 48];

% Tube Diameters (mm)
OD = [3.3, 2.64, 1.98];

% Number of speciments per tube OD
n = 5;

% Nominal Curvature (m^-1)
c_nominal = 20;

% Curvature at given times
c0 = [17.30778	18.05598	17.62079	16.81997	16.81849	17.45415	18.44607	18.2113375	17.0247	16.92785	17.12467	18.69455	18.37444	16.49196	17.03821];
c0_5 = [13.92838	15.09799	14.17737	14.06741	12.88529	14.38639	15.38363	14.57354	13.37316	14.02309	15.45006	14.83171	16.86129	13.40585	14.02348];
c1 = [13.52451	14.51855	13.35047	13.37263	12.09968	NaN	14.81256	13.8754	12.54314	13.29897	14.56303	14.65816	16.74375	12.48317	13.23512];
c1_5 = [13.19151	14.02515	12.9556	13.01089	11.58462	13.29171	14.60376	13.56491	12.08651	12.8246	14.09424	14.01165	16.42812	11.89451	12.68456];
c2 = [12.80267	13.63128	12.73062	12.76704	11.14581	13.00526	14.17639	13.32311	11.76491	12.51925	13.95657	13.60454	16.12908889	11.6693	12.32655];
c4 = [12.25764	13.2703	12.08258	12.37369	10.47633	12.59474	13.852875	13.1974	11.08627	12.06393	13.4533	12.98155	16.1265	10.75319	11.29866];
c24 = [10.48561	11.83083	10.43835	11.12082	8.73663	11.07605	12.85178	11.47414	9.23158	11.13155	13.17908	11.64981	15.3303	8.67957	9.3492];
c48 = [10.12283	11.48116	9.97724	9.80391	7.939	10.72232	12.26865	10.93027	8.17978	9.90788	12.52505	10.72055	14.8445	7.60754	8.2595];

% Calculate the Bending Radius: Nominal and at t+0hr, t+0.5hr, and t+4hr (mm)
r_nominal = 1/c_nominal * 1000;
r0 = 1./c0 * 1000;
r0_5 = 1./c0_5 * 1000;
r1 = 1./c1 * 1000;
r1_5 = 1./c1_5 * 1000;
r2 = 1./c2 * 1000;
r4 = 1./c4 * 1000;
r24 = 1./c24 * 1000;
r48 = 1./c48 * 1000;

% % Calculate the rate of Bending Radius Loss (mm/hour)
% rate1 = (r0_5 - r0) / 0.5;
% rate2 = (r1 - r0_5) / 0.5;
% rate3 = (r4 - r1) / 3.0;
% rate4 = (r24 - r4) / 21.0;
% rate5 = (r48 - r24) / 24.0;

%% Formatting

titleSize = 24;
axisLabelSize = 20;
legendSize = 20;
tickSize = 16;
lineWidth = 2.5;

%% Plot curvature loss by batch

% Plot the loss of curvature over time
for jj = 1 : n
    figure('WindowState','fullscreen'); grid on; hold on;
    title(["Progressive Loss of Curvature After Manufacturing for Tubes in batch #" num2str(jj)], "FontSize",titleSize);
    
    for ii = 1 : length(OD)
        radii = [r0(jj+n*(ii-1)) r0_5(jj+n*(ii-1)) r1(jj+n*(ii-1)) r1_5(jj+n*(ii-1)) r2(jj+n*(ii-1)) r4(jj+n*(ii-1)) r24(jj+n*(ii-1)) r48(jj+n*(ii-1))];
        plot(t(isfinite(radii)), radii(isfinite(radii)),"-o","LineWidth",lineWidth);
        xlabel("Time Since Heat-Set [Hours]", "FontSize", axisLabelSize);
        ylabel("Bending Radius [mm]", "FontSize", axisLabelSize);
        ylim([45 140]);
    end

    plot([0 t(end)], [r_nominal, r_nominal], "LineWidth", 2.5, "Color", "red");

    legend({"Thickest (OD=3.3 mm)", "Thicker (OD=2.64 mm)", "Thinner (OD=1.98 mm)", "Nominal"}, "Location", "northwest");

    saveas(gcf,"./relaxation/batch"+num2str(jj)+"_extra.png");
    saveas(gcf,"./relaxation/batch"+num2str(jj)+"_extra.svg");
end

%% Plot the loss of curvature over time / group by tube OD

tube_type = {"Thickest", "Thicker", "Thinnest"};

for jj = 1 : length(OD)
    figure('WindowState','fullscreen'); grid on; hold on;
    
    for ii = 1 : n
        radii = [r0(ii+n*(jj-1)) r0_5(ii+n*(jj-1)) r1(ii+n*(jj-1)) r1_5(ii+n*(jj-1)) r2(ii+n*(jj-1)) r4(ii+n*(jj-1)) r24(ii+n*(jj-1)) r48(ii+n*(jj-1))];
        plot(t(isfinite(radii)), radii(isfinite(radii)),"-o","LineWidth",lineWidth);
    end

    plot([0 t(end)], [r_nominal, r_nominal], "LineWidth", 2.5, "Color", "red");

    ax=gca;
    ax.FontSize = tickSize;

    xlabel("Time Since Heat-Set [Hours]", "FontSize", axisLabelSize);
    ylabel("Bending Radius [mm]", "FontSize", axisLabelSize)
    ylim([45 140]);

    title_string = ["Progressive Loss of Curvature After Manufacturing" "for Tubes with Outer Diameter " + num2str(OD(jj)) + " mm"];
    title(title_string, "FontSize",titleSize);

    legend({"Specimen 1", "Specimen 2", "Specimen 3", "Specimen 4", "Specimen 5", "Nominal"}, "Location", "northwest", "FontSize", legendSize);

    saveas(gcf,"./relaxation/"+tube_type{jj}+"_extra.png");
    saveas(gcf,"./relaxation/"+tube_type{jj}+"_extra.svg");
end

% %% Plot all  the tubes on one figure
% 
% figure('WindowState','maximized'); grid on; hold on;
% 
% tube_type = {"Thickest", "Thicker", "Thinnest"};
% 
% plot([0 t(end)], [r_nominal, r_nominal], "LineWidth", 2.5, "Color", "red");
% 
% for jj = 1 : n
%     for ii = 1 : length(OD)
%         radii = [r0(jj+n*(ii-1)) r0_5(jj+n*(ii-1)) r1(jj+n*(ii-1)) r4(jj+n*(ii-1)) r24(jj+n*(ii-1)) r48(jj+n*(ii-1))];
%         line = plot(t(isfinite(radii)), radii(isfinite(radii)),"-o","LineWidth",lineWidth);
%         line.SeriesIndex = ii + 1;
%     end
% end
% 
% ax=gca;
% ax.FontSize = tickSize;
% 
% xlabel("Time Since Heat-Set [Hours]", "FontSize", axisLabelSize);
% ylabel("Bending Radius [mm]", "FontSize", axisLabelSize)
% ylim([45 140]);
% 
% title_string = "Progressive Loss of Curvature Over Time After Heat-Setting";
% title(title_string, "FontSize",titleSize);
% 
% legend({"Nominal", "Thickest (OD=3.3 mm)", "Thicker (OD=2.64 mm)", "Thinner (OD=1.98 mm)"}, "Location", "northwest");
% 
% saveas(gcf,"./relaxation/"+"relaxation_all"+".png");

%% Make a boxplot with all 15 tubes

figure('WindowState','maximized'); grid on; hold on;

boxplot_times = [0 24 48];
c_box = [c0; c24; c48]';
r_box = 1000 ./ c_box;

boxplot(r_box, boxplot_times);
plot([0.5 3.5], [r_nominal, r_nominal], "LineWidth", 2.5, "Color", "red");

ax=gca;
ax.FontSize = tickSize;

title_string = "Progressive Loss of Curvature Over Time After Heat-Setting";
title(title_string, "FontSize",titleSize);

xlabel("Time Since Heat-Set [Hours]", "FontSize", axisLabelSize);
ylabel("Bending Radius [mm]", "FontSize", axisLabelSize)
ylim([45 140]);
xlim([0.5 3.5]);

legend({"Nominal Bending Radius"}, "Location", "northwest");

saveas(gcf,"./relaxation/"+"relaxation_box"+".png");
